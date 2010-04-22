using System;
using StatLight.Core.Events;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Timing;

namespace StatLight.Core.Monitoring
{
    public class BrowserCommunicationTimeoutMonitor
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ITimer _maxTimeoutTimer;
        private readonly TimeSpan _maxTimeAllowedBeforeCommunicationErrorSent;
        private DateTime _lastTimeAnyEventArrived;
        private bool _hasPublishedEvent;

        public BrowserCommunicationTimeoutMonitor(IEventAggregator eventAggregator, ITimer maxTimeoutTimer, TimeSpan maxTimeAllowedBeforeCommunicationErrorSent)
        {
            if (maxTimeoutTimer == null) throw new ArgumentNullException("maxTimeoutTimer");
            _eventAggregator = eventAggregator;
            _maxTimeoutTimer = maxTimeoutTimer;
            _maxTimeAllowedBeforeCommunicationErrorSent = maxTimeAllowedBeforeCommunicationErrorSent;

            _maxTimeoutTimer.Elapsed += maxTimeoutTimer_Elapsed;
            _lastTimeAnyEventArrived = DateTime.Now;


            _eventAggregator
                .AddListener<TestRunCompletedServerEvent>(e => _maxTimeoutTimer.Stop());

            _eventAggregator
                .AddListener<DialogAssertionServerEvent>(ResetTimer);

            _eventAggregator
                .AddListener<MessageReceivedFromClientServerEvent>(ResetTimer);

            _maxTimeoutTimer.Start();
        }

        void maxTimeoutTimer_Elapsed(object sender, TimerWrapperElapsedEventArgs e)
        {
            var ticksElapsedSinceLastMessage = e.SignalTime.Ticks - _lastTimeAnyEventArrived.Ticks;

            if (!_hasPublishedEvent)
            {
                if (ticksElapsedSinceLastMessage > _maxTimeAllowedBeforeCommunicationErrorSent.Ticks)
                {
                    _hasPublishedEvent = true;

                    _eventAggregator
                        .SendMessage(
                            new BrowserHostCommunicationTimeoutServerEvent
                                {
                                    Message = "No communication from the web browser has been detected. We've waited longer than the configured time of {0}".FormatWith(new TimeSpan(_maxTimeAllowedBeforeCommunicationErrorSent.Ticks))
                                }
                        );

                    _eventAggregator
                        .SendMessage<TestRunCompletedServerEvent>();
                }
            }
        }

        private void ResetTimer()
        {
            _lastTimeAnyEventArrived = DateTime.Now;
        }
    }
}