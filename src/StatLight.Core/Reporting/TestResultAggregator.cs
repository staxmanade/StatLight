

using System.Linq;

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
        IListener<TestExecutionMethodBeginClientEvent>
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly TestReport _currentReport = new TestReport();
        private readonly DialogAssertionMatchMaker _dialogAssertionMessageMatchMaker = new DialogAssertionMatchMaker();
        private readonly EventMatchMacker _eventMatchMacker = new EventMatchMacker();

        public TestResultAggregator(ILogger logger, IEventAggregator eventAggregator)
        {
            //System.Diagnostics.Debugger.Break();
            _logger = logger;
            _eventAggregator = eventAggregator;
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
            Action action = () =>
            {
                //_eventMatchMacker
                _logger.Debug(
                    "TestExecutionMethodPassedClientEvent - {0}"
                        .FormatWith(message.MethodName));
                if (
                    _dialogAssertionMessageMatchMaker.
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
            Action action = () =>
            {
                _logger.Debug("TestExecutionMethodFailedClientEvent - {0}".FormatWith(message.MethodName));

                if (_dialogAssertionMessageMatchMaker.WasEventAlreadyClosed(message))
                {
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

                ReportIt(msg);
            };

            _eventMatchMacker.AddEvent(message, action);
        }

        public void Handle(TestExecutionMethodIgnoredClientEvent message)
        {
            _logger.Debug("TestExecutionMethodIgnoredClientEvent - {0}".FormatWith(message.MethodName));
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
                _dialogAssertionMessageMatchMaker.AddAssertionHandler(message, handler);
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
            var msg = new TestCaseResult(ResultType.Failed)
            {
                OtherInfo = message.Message,
            };

            ReportIt(msg);
        }

        public void Handle(TestExecutionMethodBeginClientEvent message)
        {
            _logger.Debug("TestExecutionMethodBeginClientEvent - {0}".FormatWith(message.MethodName));
            _dialogAssertionMessageMatchMaker.HandleMethodBeginClientEvent(message);

            _eventMatchMacker.AddBeginEvent(message);
        }

        private void ReportIt(TestCaseResult result)
        {
            _currentReport.AddResult(result);
            _eventAggregator.SendMessage(result);
        }

        private class EventMatchMacker
        {
            private Dictionary<TestExecutionMethod, Action> _awaitingForAMatch =
                new Dictionary<TestExecutionMethod, Action>();

            public void AddEvent(TestExecutionMethod clientEvent, Action actionOnCompletion)
            {
                if (_awaitingForAMatch.ContainsKey(clientEvent))
                {
                    _awaitingForAMatch.Remove(clientEvent);
                    actionOnCompletion();
                }
                else
                {
                    _awaitingForAMatch.Add(clientEvent, actionOnCompletion);
                }
            }

            public void AddBeginEvent(TestExecutionMethodBeginClientEvent clientEvent)
            {
                if (_awaitingForAMatch.ContainsKey(clientEvent))
                {
                    _awaitingForAMatch[clientEvent]();
                    _awaitingForAMatch.Remove(clientEvent);
                }
                else
                {
                    _awaitingForAMatch.Add(clientEvent, null);
                }
            }
        }

    }
}
