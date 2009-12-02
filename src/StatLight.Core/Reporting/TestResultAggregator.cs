
namespace StatLight.Core.Reporting
{
	using Microsoft.Practices.Composite.Events;
	using StatLight.Core.Events;
	using StatLight.Core.Reporting.Messages;
	using System;

	public class TestResultAggregator : IDisposable
	{
		private readonly ITestResultHandler _testResultHandler;
		private readonly IEventAggregator _eventAggregator;

		private readonly TestReport _currentReport = new TestReport();
		private readonly SubscriptionToken _testResultSubscriptionToken;
		private readonly SubscriptionToken _dialogAssertionSubscriptionToken;
		private readonly SubscriptionToken _testHarnessOtherMessageSubscriptionToken;
		private readonly SubscriptionToken _browserHostCommunicationTimeoutSubscriptionToken;

		public TestReport CurrentReport { get { return _currentReport; } }

		public TestResultAggregator(ITestResultHandler testResultHandler, IEventAggregator eventAggregator)
		{
			this._testResultHandler = testResultHandler;
			this._eventAggregator = eventAggregator;

			_testResultSubscriptionToken = eventAggregator
				.GetEvent<TestResultEvent>()
				.Subscribe(OnMobilScenarioResult_Received);

			_dialogAssertionSubscriptionToken = eventAggregator
				.GetEvent<DialogAssertionEvent>()
				.Subscribe(OnMobilScenarioResult_Received);

			_testHarnessOtherMessageSubscriptionToken = eventAggregator
				.GetEvent<TestHarnessOtherMessageEvent>()
				.Subscribe(OnMobilOtherMessageType_Received);

			_browserHostCommunicationTimeoutSubscriptionToken = eventAggregator
				.GetEvent<BrowserHostCommunicationTimeoutEvent>()
				.Subscribe(OnBrowserHostCommunicationTimeout_Received);
		}

		private void OnMobilScenarioResult_Received(MobilScenarioResult message)
		{
			_currentReport.AddResult(message);
			_testResultHandler.HandleMessage(message);
		}

		private void OnMobilOtherMessageType_Received(MobilOtherMessageType message)
		{
			_currentReport.AddResult(message);
			_testResultHandler.HandleMessage(message);
		}

		private void OnBrowserHostCommunicationTimeout_Received(EmptyPayload obj)
		{
			var timeoutMessage = new MobilOtherMessageType()
			{
				Message = "Any form of communication from the browser/xap has not occured in the configured timeout period.",
				MessageType = LogMessageType.Error,
			};
			_currentReport.AddResult(timeoutMessage);
			_testResultHandler.HandleMessage(timeoutMessage);
		}

		#region IDisposable Members

		public void Dispose()
		{
			_eventAggregator
				.GetEvent<TestResultEvent>()
				.Unsubscribe(_testResultSubscriptionToken);

			_eventAggregator
				.GetEvent<DialogAssertionEvent>()
				.Unsubscribe(_dialogAssertionSubscriptionToken);

			_eventAggregator
				.GetEvent<TestHarnessOtherMessageEvent>()
				.Unsubscribe(_testHarnessOtherMessageSubscriptionToken);

			_eventAggregator
				.GetEvent<BrowserHostCommunicationTimeoutEvent>()
				.Unsubscribe(_browserHostCommunicationTimeoutSubscriptionToken);


			GC.SuppressFinalize(this);
		}

		#endregion
	}
}
