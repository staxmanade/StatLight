

namespace StatLight.Core.Reporting
{
    using System;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Common;
    using StatLight.Core.Events;
    using StatLight.Core.Events.Aggregation;
    using System.Collections.Generic;

    public class TestResultAggregator : IDisposable,
        IListener<TestExecutionMethodPassedClientEvent>,
        IListener<TestExecutionMethodFailedClientEvent>,
        IListener<TestExecutionMethodIgnoredClientEvent>,
        IListener<TraceClientEvent>,
        IListener<DialogAssertionServerEvent>,
        IListener<BrowserHostCommunicationTimeoutServerEvent>,
        IListener<FatalSilverlightExceptionServerEvent>,
        IListener<TestExecutionMethodBeginClientEvent>,
        IListener<UnhandledExceptionClientEvent>
    {
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly TestReport _currentReport;
        private readonly DialogAssertionMatchmaker _dialogAssertionMessageMatchmaker = new DialogAssertionMatchmaker();
        private readonly EventMatchMacker _eventMatchMacker;
        private readonly List<int> _beginEventsAlreadyFired = new List<int>();

        public TestResultAggregator(ILogger logger, IEventPublisher eventPublisher, string xapPath)
        {
            //System.Diagnostics.Debugger.Break();
            _logger = logger;
            _eventPublisher = eventPublisher;
            _currentReport = new TestReport(xapPath);
            _eventMatchMacker = new EventMatchMacker(_logger);
        }

        public TestReport CurrentReport { get { return _currentReport; } }

        #region IDisposable Members

        public void Dispose()
        {

            //TODO: stall while events are still arriving???
        }

        #endregion

        public void Handle(TestExecutionMethodPassedClientEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            //_logger.Debug("Handle - TestExecutionMethodPassedClientEvent - {0}".FormatWith(message.FullMethodName));
            Action action = () =>
            {
                if (
                    _dialogAssertionMessageMatchmaker.
                        WasEventAlreadyClosed(message))
                {
                    // Don't include this as a "passed" test as we had to automatically close the dialog);)
                    return;
                }

                var msg = new TestCaseResult(ResultType.Passed)
                {
                    Finished = message.Finished,
                    Started = message.Started,
                };

                TranslateCoreInfo(ref msg, message);

                ReportIt(msg);
            };

            _eventMatchMacker.AddEvent(message, action);
        }

        private static void TranslateCoreInfo(ref TestCaseResult result, TestExecutionMethod message)
        {
            result.ClassName = message.ClassName;
            result.NamespaceName = message.NamespaceName;
            result.MethodName = message.MethodName;
        }

        public void Handle(TestExecutionMethodFailedClientEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            //_logger.Debug("Handle - TestExecutionMethodFailedClientEvent - {0}".FormatWith(message.FullMethodName));
            Action action = () =>
            {
                if (_dialogAssertionMessageMatchmaker.WasEventAlreadyClosed(message))
                {
                    // Don't include this as a "passed" test as we had to automatically close the dialog);)
                    return;
                }

                var msg = new TestCaseResult(ResultType.Failed)
                {
                    Finished = message.Finished,
                    Started = message.Started,
                    ExceptionInfo = message.ExceptionInfo,
                    OtherInfo = message.Description,
                };

                TranslateCoreInfo(ref msg, message);

                ReportIt(msg);
            };

            _eventMatchMacker.AddEvent(message, action);
        }

        public void Handle(TestExecutionMethodIgnoredClientEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            //_logger.Debug("Handle - TestExecutionMethodIgnoredClientEvent - {0}".FormatWith(message.FullMethodName));
            var msg = new TestCaseResult(ResultType.Ignored)
            {
                MethodName = message.Message,
            };
            ReportIt(msg);
        }

        public void Handle(TraceClientEvent message)
        {
            //TODO: add to TestReport???
        }

        public void Handle(DialogAssertionServerEvent message)
        {
            Action<TestExecutionMethodBeginClientEvent> handler = m =>
            {
                var namespaceName = m.NamespaceName;
                var className = m.ClassName;
                var methodName = m.MethodName;

                var msg = new TestCaseResult(ResultType.Failed)
                {
                    OtherInfo = message.Message,
                    NamespaceName = namespaceName,
                    ClassName = className,
                    MethodName = methodName,
                };

                ReportIt(msg);
            };

            if (message.DialogType == DialogType.Assert)
            {
                _dialogAssertionMessageMatchmaker.AddAssertionHandler(message, handler);
            }
            else if (message.DialogType == DialogType.MessageBox)
            {
                var msg = new TestCaseResult(ResultType.SystemGeneratedFailure)
                {
                    OtherInfo = message.Message,
                    NamespaceName = "[StatLight]",
                    ClassName = "[CannotFigureItOut]",
                    MethodName = "[NotEnoughContext]",
                };

                ReportIt(msg);
            }
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            string messageValue = message.Message;

            ReportFailureMessage(messageValue);
        }

        public void Handle(TestExecutionMethodBeginClientEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");

            if (_beginEventsAlreadyFired.Contains(message.FullMethodName.GetHashCode()))
            {
                _logger.Debug(message.WriteDebug());

                return;
            }

            _beginEventsAlreadyFired.Add(message.FullMethodName.GetHashCode());

            //_logger.Debug("Handle - TestExecutionMethodBeginClientEvent - {0}".FormatWith(message.FullMethodName));
            _dialogAssertionMessageMatchmaker.HandleMethodBeginClientEvent(message);

            _eventMatchMacker.AddBeginEvent(message);
        }

        private void ReportIt(TestCaseResult result)
        {
            _currentReport.AddResult(result);
            _eventPublisher.SendMessage(result);
        }

        private class EventMatchMacker
        {
            private readonly ILogger _logger;

            public EventMatchMacker(ILogger logger)
            {
                _logger = logger;
            }
            private static readonly object _sync = new object();
            private readonly Dictionary<string, Action> _awaitingForAMatch = new Dictionary<string, Action>();

            public void AddEvent(TestExecutionMethod clientEvent, Action actionOnCompletion)
            {
                lock (_sync)
                {
                    if (_awaitingForAMatch.ContainsKey(clientEvent.FullMethodName))
                    {
                        _awaitingForAMatch.Remove(clientEvent.FullMethodName);
                        actionOnCompletion();
                    }
                    else
                    {
                        _awaitingForAMatch.Add(clientEvent.FullMethodName, actionOnCompletion);
                    }
                }
            }

            public void AddBeginEvent(TestExecutionMethodBeginClientEvent clientEvent)
            {
                lock (_sync)
                {
                    if (_awaitingForAMatch.ContainsKey(clientEvent.FullMethodName))
                    {
                        Action a = _awaitingForAMatch[clientEvent.FullMethodName];
                        if (a != null)
                        {
                            a();
                        }
                        _awaitingForAMatch.Remove(clientEvent.FullMethodName);
                    }
                    else
                    {
                        _awaitingForAMatch.Add(clientEvent.FullMethodName, null);
                    }
                }
            }
        }

        public void Handle(FatalSilverlightExceptionServerEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");

            string messageValue = message.Message;

            ReportFailureMessage(messageValue);
        }

        private void ReportFailureMessage(string messageValue)
        {
            var msg = new TestCaseResult(ResultType.Failed)
                          {
                              OtherInfo = messageValue,
                          };

            ReportIt(msg);
        }

        public void Handle(UnhandledExceptionClientEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            if (message.ExceptionInfo == null) return;

            string messageValue = message.ExceptionInfo.FullMessage;

            ReportFailureMessage(messageValue);
        }
    }
}
