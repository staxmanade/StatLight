using System;
using Microsoft.Practices.Composite.Events;
using StatLight.Core.Events;
using StatLight.Core.Timing;

namespace StatLight.Core.Monitoring
{
	public class BrowserCommunicationTimeoutMonitor
	{
		private readonly IEventAggregator _eventAggregator;
		private readonly ITimer _maxTimeoutTimer;
		private readonly TimeSpan _maxTimeAllowedBeforeCommErrorSent;
		private DateTime _lastTimeAnyEventArrived;
		private bool _hasPublishedEvent;

		public BrowserCommunicationTimeoutMonitor(IEventAggregator eventAggregator, ITimer maxTimeoutTimer, TimeSpan maxTimeAllowedBeforeCommErrorSent)
		{
			_eventAggregator = eventAggregator;
			_maxTimeoutTimer = maxTimeoutTimer;
			_maxTimeAllowedBeforeCommErrorSent = maxTimeAllowedBeforeCommErrorSent;

			_maxTimeoutTimer.Elapsed += maxTimeoutTimer_Elapsed;
			_lastTimeAnyEventArrived = DateTime.Now;

			_eventAggregator
				.GetEvent<TestRunCompletedEvent>()
				.Subscribe((e) => _maxTimeoutTimer.Stop());

			_eventAggregator
				.GetEvent<TestResultEvent>()
				.Subscribe(ResetTimer);

			_eventAggregator
				.GetEvent<DialogAssertionEvent>()
				.Subscribe(ResetTimer);

			_eventAggregator
				.GetEvent<TestHarnessOtherMessageEvent>()
				.Subscribe(ResetTimer);

			_maxTimeoutTimer.Start();
		}

		void maxTimeoutTimer_Elapsed(object sender, TimerWrapperElapsedEventArgs e)
		{
			var ticksElapsedSinceLastMessage = e.SignalTime.Ticks - _lastTimeAnyEventArrived.Ticks;

			if (!_hasPublishedEvent)
			{
				if (ticksElapsedSinceLastMessage > _maxTimeAllowedBeforeCommErrorSent.Ticks)
				{
					_hasPublishedEvent = true;

					_eventAggregator
						.GetEvent<BrowserHostCommunicationTimeoutEvent>()
						.Publish();

					_eventAggregator
						.GetEvent<TestRunCompletedEvent>()
						.Publish();
				}
			}
		}

		private void ResetTimer(object anyEvent)
		{
			_lastTimeAnyEventArrived = DateTime.Now;
		}
	}
}