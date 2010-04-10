using System;
using System.Collections.Generic;
using System.Linq;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;

namespace StatLight.Core.Reporting
{
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
}