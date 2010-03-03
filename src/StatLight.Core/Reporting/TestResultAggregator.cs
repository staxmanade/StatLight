
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Reporting
{
    using System;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Events;
    using StatLight.Core.Common;

    public class TestResultAggregator : IDisposable,
        ITestingReportEvents,
        IListener<TestExecutionMethodBeginClientEvent>
    {
        private readonly ILogger _logger;
        private readonly TestReport _currentReport = new TestReport();
        public TestReport CurrentReport { get { return _currentReport; } }
        private TestExecutionMethodBeginClientEvent _mostRecentBeginEventMessage;

        public TestResultAggregator(ILogger logger)
        {
            _logger = logger;
           // Debugger.Break();
        }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public void Handle(TestExecutionMethodPassedClientEvent message)
        {
            _logger.Debug(message.FullMethodName());

            //if (_methodsToIgnorePassedMessageFor
            //    .Any(w =>
            //        w.NamespaceName == message.NamespaceName
            //        && w.ClassName == message.ClassName
            //         && w.MethodName == message.MethodName
            //        )
            //    )
            //{
            //    _logger.Warning("********Skipped passing test message for {1}".FormatWith(message.FullMethodName()));
            //    // Don't include this as a "passed" test as we had to automatically close the dialog);)
            //    return;
            //}

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
            _logger.Debug(message.FullMethodName());

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
            _logger.Debug(message.FullMethodName());

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
            string namespaceName = null;
            string className = null;
            string methodName = null;

            if (_mostRecentBeginEventMessage != null)
            {
                //_methodsToIgnorePassedMessageFor.Add(_mostRecentBeginEventMessage);

                namespaceName = _mostRecentBeginEventMessage.NamespaceName;
                className = _mostRecentBeginEventMessage.ClassName;
                methodName = _mostRecentBeginEventMessage.MethodName;
            }

            var msg = new TestCaseResult(ResultType.Failed)
            {
                OtherInfo = message.Message,
                NamespaceName = namespaceName,
                ClassName = className,
                MethodName = methodName,
            };

            _currentReport.AddResult(msg);
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
            var msg = new TestCaseResult(ResultType.Failed)
            {
                OtherInfo = message.Message,
            };

            _currentReport.AddResult(msg);
        }

        public void Handle(TestExecutionMethodBeginClientEvent message)
        {
            _logger.Debug(message.FullMethodName());

            //waitingForResults.Add(message);

            _mostRecentBeginEventMessage = message;
        }

        private class WaitingMessageGroupings
        {
            private readonly List<TestExecutionMethodBeginClientEvent> _waitingFor =
                new List<TestExecutionMethodBeginClientEvent>();

            private DialogAssertionServerEvent currentDialogServerEvent;

            public void Add(TestExecutionMethodBeginClientEvent message)
            {
                _waitingFor.Add(message);
            }

            public void Apply(TestExecutionMethod message)
            {
                
            }

            public void Add(DialogAssertionServerEvent message)
            {
                currentDialogServerEvent = message;
            }
        }
    }

    public static class ext
    {
        public static string FullMethodName(this TestExecutionMethod value)
        {
            return "{0}.{1}.{2}".FormatWith(value.NamespaceName, value.ClassName, value.MethodName);
        }
    }
}
