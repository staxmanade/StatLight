
using System.Collections.Generic;

namespace StatLight.Core.Reporting
{
    using System;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Events;

    public class TestResultAggregator : IDisposable, ITestingReportEvents
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
            //TODO: add to TestReport???
        }

        public void Handle(DialogAssertionServerEvent message)
        {
            //TODO:
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
            var timeoutMessage = new BrowserHostCommunicationTimeoutResult { };
            _currentReport.AddResult(timeoutMessage);
        }
    }
}
