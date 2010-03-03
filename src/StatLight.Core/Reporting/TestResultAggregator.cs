

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
        ITestingReportEvents,
        IListener<TestExecutionMethodBeginClientEvent>
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly TestReport _currentReport = new TestReport();
        private readonly DialogAssertionMessageMatchMaker _dialogAssertionMessageMatchMaker = new DialogAssertionMessageMatchMaker();
        private List<SelfManufacturedFailureEvent> _selfManufacturedEvents = new List<SelfManufacturedFailureEvent>();
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
            _logger.Debug(message.FullMethodName());

            if (_dialogAssertionMessageMatchMaker.WasEventAlreadyClosed(message))
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

                _currentReport.AddResult(msg);

                var selfManufacturedFailureEvent = new SelfManufacturedFailureEvent(msg);
                if (!_selfManufacturedEvents.Contains(selfManufacturedFailureEvent))
                {
                    var newFailureEvent = new TestExecutionMethodFailedClientEvent
                    {
                        NamespaceName = namespaceName,
                        ClassName = className,
                        MethodName = methodName,
                        ExceptionInfo = new Exception(message.Message),
                    };

                    _selfManufacturedEvents.Add(selfManufacturedFailureEvent);
                    _eventAggregator.SendMessage(newFailureEvent);
                }

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


        private class SelfManufacturedFailureEvent : IEquatable<SelfManufacturedFailureEvent>
        {
            public SelfManufacturedFailureEvent(TestCaseResult e)
            {
                NamespaceName = e.NamespaceName;
                ClassName = e.ClassName;
                MethodName = e.MethodName;
                OtherInfo = e.OtherInfo;
            }
            public string NamespaceName { get; set; }
            public string ClassName { get; set; }
            public string MethodName { get; set; }
            public string OtherInfo { get; set; }

            public bool Equals(SelfManufacturedFailureEvent other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(other.NamespaceName, NamespaceName) && Equals(other.ClassName, ClassName) && Equals(other.MethodName, MethodName) && Equals(other.OtherInfo, OtherInfo);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != typeof(SelfManufacturedFailureEvent)) return false;
                return Equals((SelfManufacturedFailureEvent)obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int result = (NamespaceName != null ? NamespaceName.GetHashCode() : 0);
                    result = (result * 397) ^ (ClassName != null ? ClassName.GetHashCode() : 0);
                    result = (result * 397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
                    result = (result * 397) ^ (OtherInfo != null ? OtherInfo.GetHashCode() : 0);
                    return result;
                }
            }

            public static bool operator ==(SelfManufacturedFailureEvent left, SelfManufacturedFailureEvent right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(SelfManufacturedFailureEvent left, SelfManufacturedFailureEvent right)
            {
                return !Equals(left, right);
            }
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
