
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
        private DialogAssertionMessageMatchMaker _dialogAssertionMessageMatchMaker = new DialogAssertionMessageMatchMaker();

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

            if (_dialogAssertionMessageMatchMaker.WasEventAlreadyClosed(message))
            {
                _logger.Warning("********Skipped passing test message for {0}".FormatWith(message.FullMethodName()));
                // Don't include this as a "passed" test as we had to automatically close the dialog);)
                return;
            }


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

            if (_dialogAssertionMessageMatchMaker.WasEventAlreadyClosed(message))
            {
                _logger.Warning("********Skipped passing test message for {0}".FormatWith(message.FullMethodName()));
                // Don't include this as a "passed" test as we had to automatically close the dialog);)
                return;
            }

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
            Action<TestExecutionMethodBeginClientEvent> handler = m =>
            {
                var msg = new TestCaseResult(ResultType.Failed)
                {
                    OtherInfo = message.Message,
                    NamespaceName = m.NamespaceName,
                    ClassName = m.ClassName,
                    MethodName = m.MethodName,
                };

                _currentReport.AddResult(msg);
            };

            _dialogAssertionMessageMatchMaker.Handle(message, handler);
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

            _dialogAssertionMessageMatchMaker.Handle(message);
        }
    }


    public class DialogAssertionMessageMatchMaker
    {
        private readonly List<TestExecutionMethodBeginClientEvent> _completedMessage = new List<TestExecutionMethodBeginClientEvent>();

        private TestExecutionMethodBeginClientEvent _currentBeginEvent;
        private DialogAssertionServerEvent _currentDialogServerEvent;
        private Action<TestExecutionMethodBeginClientEvent> _onMatched;

        public void Handle(TestExecutionMethodBeginClientEvent message)
        {
            if (_currentDialogServerEvent != null)
            {
                _onMatched(message);
                ResetWithBeginEvent(message);
            }
            else
            {
                _currentBeginEvent = message;
            }
        }

        public void Handle(DialogAssertionServerEvent message, Action<TestExecutionMethodBeginClientEvent> onMatched)
        {
            if (_currentBeginEvent != null)
            {
                onMatched(_currentBeginEvent);
                ResetWithBeginEvent(_currentBeginEvent);
            }
            else
            {
                _currentDialogServerEvent = message;
                _onMatched = onMatched;
            }
        }

        public bool WasEventAlreadyClosed(TestExecutionMethod message)
        {
            return _completedMessage.Any(a =>
                                  a.NamespaceName == message.NamespaceName
                                  && a.ClassName == message.ClassName
                                  && a.MethodName == message.MethodName);
        }

        private void ResetWithBeginEvent(TestExecutionMethodBeginClientEvent message)
        {
            _completedMessage.Add(message);
            _currentBeginEvent = null;
            _currentDialogServerEvent = null;
            _onMatched = null;
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
