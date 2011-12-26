using System;
using System.Collections.Generic;
using System.Linq;
using StatLight.Core.Events;

namespace StatLight.Core.Reporting
{
    public class DialogAssertionMatchmaker
    {
        private readonly List<TestExecutionMethodBeginClientEvent> _completedMessage = new List<TestExecutionMethodBeginClientEvent>();
        private readonly Dictionary<DialogAssertionServerEvent, Action<TestExecutionMethodBeginClientEvent>> _dialogAssertionEventsWithHandlers = new Dictionary<DialogAssertionServerEvent, Action<TestExecutionMethodBeginClientEvent>>();
        private readonly List<TestExecutionMethodBeginClientEvent> _notHandledYet = new List<TestExecutionMethodBeginClientEvent>();

        public void HandleMethodBeginClientEvent(TestExecutionMethodBeginClientEvent message)
        {
            var searchFor = GetItemToSearchFor(message);

            if (_dialogAssertionEventsWithHandlers.Any(w => w.Key.Message.Contains(searchFor)))
            {
                var x = _dialogAssertionEventsWithHandlers.First(w => w.Key.Message.Contains(searchFor));
                MarkItMatched(message, x.Value);
            }
            else
            {
                _notHandledYet.Add(message);
            }
        }

        public void AddAssertionHandler(DialogAssertionServerEvent message, Action<TestExecutionMethodBeginClientEvent> onMatched)
        {
            var item = _notHandledYet.FirstOrDefault(begin => message.Message.Contains(GetItemToSearchFor(begin)));

            if(item ==  null)
            {
                _dialogAssertionEventsWithHandlers.Add(message, onMatched);
            }
            else
            {
                _notHandledYet.Remove(item);
                MarkItMatched(item, onMatched);
            }
        }

        public bool WasEventAlreadyClosed(TestExecutionMethodClientEvent message)
        {
            return _completedMessage.Any(a =>
                                         a.NamespaceName == message.NamespaceName
                                         && a.ClassName == message.ClassName
                                         && a.MethodName == message.MethodName);
        }

        private static string GetItemToSearchFor(TestExecutionMethodBeginClientEvent message)
        {
            return message.ClassName + "." + message.MethodName;
        }

        private void MarkItMatched(TestExecutionMethodBeginClientEvent message, Action<TestExecutionMethodBeginClientEvent> action)
        {
            _completedMessage.Add(message);
            action(message);
        }
    }
}