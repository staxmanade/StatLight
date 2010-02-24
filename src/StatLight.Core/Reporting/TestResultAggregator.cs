
using StatLight.Client.Harness.Events;
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Reporting
{
    using StatLight.Core.Events;
    using System;

    public class TestResultAggregator : IDisposable,
        IListener<TestExecutionMethodPassedClientEvent>,
        IListener<TestExecutionMethodFailedClientEvent>,
        IListener<TestExecutionMethodIgnoredClientEvent>,
        IListener<TraceClientEvent>,
        IListener<DialogAssertionEvent>,
        IListener<BrowserHostCommunicationTimeoutEvent>
    {
        private readonly TestReport _currentReport = new TestReport();

        public TestReport CurrentReport { get { return _currentReport; } }

        public TestResultAggregator()
        {
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public void Handle(TestExecutionMethodPassedClientEvent message)
        {
            var msg = new TestCaseResult(ResultType.Passed)
                          {
                              Finished = message.Finished,
                              Started = message.Started,
                          };

            TranslateCoreInfo(ref msg, message);

            _currentReport.AddResult(msg);
        }

        private static void TranslateCoreInfo(ref TestCaseResult result, TestExecutionMethod message)
        {
            result.ClassName = message.ClassName;
            result.NamespaceName = message.NamespaceName;
            result.MethodName = message.MethodName;
        }

        public void Handle(TestExecutionMethodFailedClientEvent message)
        {
            var msg = new TestCaseResult(ResultType.Failed)
            {
                Finished = message.Finished,
                Started = message.Started,
                ExceptionInfo = message.ExceptionInfo,
            };

            TranslateCoreInfo(ref msg, message);

            _currentReport.AddResult(msg);
        }

        public void Handle(TestExecutionMethodIgnoredClientEvent message)
        {
            var msg = new TestCaseResult(ResultType.Ignored)
                          {
                              MethodName = message.Message,
                          };
            _currentReport.AddResult(msg);
        }

        public void Handle(TraceClientEvent message)
        {
            //throw new NotImplementedException();
        }

        public void Handle(DialogAssertionEvent message)
        {
            //throw new NotImplementedException();
        }

        public void Handle(BrowserHostCommunicationTimeoutEvent message)
        {
            var timeoutMessage = new BrowserHostCommunicationTimeoutResult { };
            _currentReport.AddResult(timeoutMessage);
        }
    }
}
