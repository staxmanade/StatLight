
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Reporting
{
    using StatLight.Core.Events;
    using StatLight.Core.Reporting.Messages;
    using System;

    public class TestResultAggregator : IDisposable
    {
        private readonly ITestResultHandler _testResultHandler;
        private readonly IEventAggregator _eventAggregator;

        private readonly TestReport _currentReport = new TestReport();

        public TestReport CurrentReport { get { return _currentReport; } }

        public TestResultAggregator(ITestResultHandler testResultHandler, IEventAggregator eventAggregator)
        {
            _testResultHandler = testResultHandler;
            _eventAggregator = eventAggregator;

            eventAggregator.AddListener<TestResultEvent>(message => OnMobilScenarioResult_Received(message.Payload));
            eventAggregator.AddListener<DialogAssertionEvent>(message => OnMobilScenarioResult_Received(message.Payload));
            eventAggregator.AddListener<TestHarnessOtherMessageEvent>(message => OnMobilOtherMessageType_Received(message.Payload));
            eventAggregator.AddListener<BrowserHostCommunicationTimeoutEvent>(message => OnBrowserHostCommunicationTimeout_Received());

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

        private void OnBrowserHostCommunicationTimeout_Received()
        {
            var timeoutMessage = new MobilOtherMessageType
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
        }

        #endregion
    }
}
