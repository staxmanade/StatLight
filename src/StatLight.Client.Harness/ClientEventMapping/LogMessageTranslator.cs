using System.Collections.Generic;
using Microsoft.Silverlight.Testing.Harness;
using StatLight.Client.Model.Events;

namespace StatLight.Client.Harness.ClientEventMapping
{
    public class LogMessageTranslator
    {
        private readonly List<ILogMessageToClientEventTranslation> _logMessageMappers = new List<ILogMessageToClientEventTranslation>();

        public LogMessageTranslator()
        {
            _logMessageMappers.Add(new InitializationOfUnitTestHarnessClientEventMap());
            _logMessageMappers.Add(new TestExecutionClassBeginClientEventMap());
            _logMessageMappers.Add(new TestExecutionClassCompletedClientEventMap());
            _logMessageMappers.Add(new TestExecutionMethodBeginClientEventMap());
            _logMessageMappers.Add(new TestExecutionMethodIgnoredClientEventMap());
            _logMessageMappers.Add(new TestExecutionMethodFailedClientEventMap());
        }

        public bool TryTranslate(LogMessage message, out ClientEvent clientEvent)
        {
            foreach (var map in _logMessageMappers)
            {
                if(map.CanTranslate(message))
                {
                    clientEvent = map.Translate(message);
                    return true;
                }
            }
            clientEvent = null;
            return false;
        }
    }
}