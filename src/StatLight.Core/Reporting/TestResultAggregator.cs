

namespace StatLight.Core.Reporting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Common;
    using StatLight.Core.Events;
    using StatLight.Core.Events.Aggregation;

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
        private readonly DialogMessageMatchMaker _dialogMessageMatchMaker = new DialogMessageMatchMaker();
        public TestResultAggregator(ILogger logger, IEventAggregator eventAggregator)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
        }

        public TestReport CurrentReport { get { return _currentReport; } }

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion

        public void Handle(TestExecutionMethodPassedClientEvent message)
        {
            if (_dialogAssertionMessageMatchMaker.WasEventAlreadyClosed(message))
            {
                // Don't include this as a "passed" test as we had to automatically close the dialog);)
                return;
            }

            if (_dialogMessageMatchMaker.WasEventAlreadyClosed(message))
            {
                return;
            }

            var msg = new TestCaseResult(ResultType.Passed)
                          {
                              Finished = message.Finished,
                              Started = message.Started,
                          };

            TranslateCoreInfo(ref msg, message);

            ReportIt(msg);
        }

        private static void TranslateCoreInfo(ref TestCaseResult result, TestExecutionMethod message)
        {
            result.ClassName = message.ClassName;
            result.NamespaceName = message.NamespaceName;
            result.MethodName = message.MethodName;
        }

        public void Handle(TestExecutionMethodFailedClientEvent message)
        {
            if (_dialogAssertionMessageMatchMaker.WasEventAlreadyClosed(message))
            {
                // Don't include this as a "passed" test as we had to automatically close the dialog);)
                return;
            }

            if (_dialogMessageMatchMaker.WasEventAlreadyClosed(message))
            {
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
        }

        public void Handle(TestExecutionMethodIgnoredClientEvent message)
        {
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
                _dialogAssertionMessageMatchMaker.AddAssertionHandler(message, handler);
            else if (message.DialogType == DialogType.MessageBox)
            {
                _dialogMessageMatchMaker.AddMessageBoxHandler(message, handler);
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
            _dialogAssertionMessageMatchMaker.HandleMethodBeginClientEvent(message);
            _dialogMessageMatchMaker.HandleMethodBeginClientEvent(message);
        }

        private void ReportIt(TestCaseResult result)
        {
            _currentReport.AddResult(result);
            _eventAggregator.SendMessage(result);
        }

    }


    public class DialogAssertionMatchMaker
    {
        private readonly List<TestExecutionMethodBeginClientEvent> _completedMessage = new List<TestExecutionMethodBeginClientEvent>();

        private readonly Dictionary<DialogAssertionServerEvent, Action<TestExecutionMethodBeginClientEvent>> _dialogAssertionEventsWithHandlers =
            new Dictionary<DialogAssertionServerEvent, Action<TestExecutionMethodBeginClientEvent>>();

        public void HandleMethodBeginClientEvent(TestExecutionMethodBeginClientEvent message)
        {
            if (_dialogAssertionEventsWithHandlers.Any(w => w.Key.Message.Contains(message.MethodName)))
            {
                var x = _dialogAssertionEventsWithHandlers.First(w => w.Key.Message.Contains(message.MethodName));
                _completedMessage.Add(message);
                x.Value(message);
            }
        }

        public void AddAssertionHandler(DialogAssertionServerEvent message, Action<TestExecutionMethodBeginClientEvent> onMatched)
        {
            _dialogAssertionEventsWithHandlers.Add(message, onMatched);
        }

        public bool WasEventAlreadyClosed(TestExecutionMethod message)
        {
            return _completedMessage.Any(a =>
                                         a.NamespaceName == message.NamespaceName
                                         && a.ClassName == message.ClassName
                                         && a.MethodName == message.MethodName);
        }
    }


    public class DialogMessageMatchMakerX
    {
        public void HandleMethodBeginClientEvent(TestExecutionMethodBeginClientEvent message)
        {
        }

        public void AddMessageBoxHandler(DialogAssertionServerEvent message, Action<TestExecutionMethodBeginClientEvent> onMatched)
        {
            var testExecutionMethodBeginClientEvent = new TestExecutionMethodBeginClientEvent { };
            onMatched(testExecutionMethodBeginClientEvent);
        }

        public bool WasEventAlreadyClosed(TestExecutionMethod message)
        {
            return false;
        }
    }


    public class DialogMessageMatchMaker
    {
        private readonly List<TestExecutionMethodBeginClientEvent> _completedMessage = new List<TestExecutionMethodBeginClientEvent>();

        private TestExecutionMethodBeginClientEvent _currentBeginEvent;
        private DialogAssertionServerEvent _currentDialogServerEvent;
        private Action<TestExecutionMethodBeginClientEvent> _onMatched;

        public void HandleMethodBeginClientEvent(TestExecutionMethodBeginClientEvent message)
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

        public void AddMessageBoxHandler(DialogAssertionServerEvent message, Action<TestExecutionMethodBeginClientEvent> onMatched)
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
}
