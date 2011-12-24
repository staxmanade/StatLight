using System;
using StatLight.Core.Common.Abstractions.Timing;
using StatLight.Core.Events;

namespace StatLight.Core.Monitoring
{
    public class BrowserCommunicationTimeoutMonitor : 
        IListener<TestRunCompletedServerEvent>,
        IListener<DialogAssertionServerEvent>,
        IListener<MessageReceivedFromClientServerEvent>
    {
        private readonly IEventPublisher _eventPublisher;
        private readonly ITimer _maxTimeoutTimer;
        private readonly TimeSpan _maxTimeAllowedBeforeCommunicationErrorSent;
        private DateTime _lastTimeAnyEventArrived;
        private bool _hasPublishedEvent;

        public BrowserCommunicationTimeoutMonitor(IEventPublisher eventPublisher,
            ITimer maxTimeoutTimer, TimeSpan maxTimeAllowedBeforeCommunicationErrorSent)
        {
            if (maxTimeoutTimer == null) throw new ArgumentNullException("maxTimeoutTimer");
            _eventPublisher = eventPublisher;
            _maxTimeoutTimer = maxTimeoutTimer;
            _maxTimeAllowedBeforeCommunicationErrorSent = maxTimeAllowedBeforeCommunicationErrorSent;

            _maxTimeoutTimer.Elapsed += maxTimeoutTimer_Elapsed;
            _lastTimeAnyEventArrived = DateTime.Now;


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

                    _eventPublisher
                        .SendMessage(
                            new BrowserHostCommunicationTimeoutServerEvent
                                {
                                    Message = "No communication from the web browser has been detected. We've waited longer than the configured time of {0}".FormatWith(new TimeSpan(_maxTimeAllowedBeforeCommunicationErrorSent.Ticks))
                                }
                        );

                    _eventPublisher
                        .SendMessage<TestRunCompletedServerEvent>();
                }
            }
        }

        public void Handle(TestRunCompletedServerEvent message)
        {
            _maxTimeoutTimer.Stop();
        }

        public void Handle(DialogAssertionServerEvent message)
        {
            ResetTimer();
        }

        public void Handle(MessageReceivedFromClientServerEvent message)
        {
            ResetTimer();
        }

        private void ResetTimer()
        {
            _lastTimeAnyEventArrived = DateTime.Now;
        }
    }
}