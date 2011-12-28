using System.Collections;
using System.Collections.Generic;
using Microsoft.Silverlight.Testing.Harness;
using StatLight.Core.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public class LogMessageTranslator
    {
        private readonly MappersCollection logMessageTranslator;

        public LogMessageTranslator()
        {
            logMessageTranslator = new MappersCollection();
            logMessageTranslator.Add<InitializationOfUnitTestHarnessClientEventMap>();
            logMessageTranslator.Add<TestExecutionClassBeginClientEventMap>();
            logMessageTranslator.Add<TestExecutionClassCompletedClientEventMap>();
            logMessageTranslator.Add<TestExecutionMethodBeginClientEventMap>();
            logMessageTranslator.Add<TestExecutionMethodIgnoredClientEventMap>();
            logMessageTranslator.Add<TestExecutionMethodFailedClientEventMap>();
            logMessageTranslator.Add<TestExecutionMethodExpectedExcaptionFailedClientEventMap>();
            logMessageTranslator.Add<TestExecutionMethodPassedClientEventMap>();
            logMessageTranslator.Add<TestExecutionDoNotReportMessageMap>();
        }

        public bool TryTranslate(LogMessage message, out ClientEvent clientEvent)
        {
            foreach (var map in logMessageTranslator)
            {
                if (!map.CanTranslate(message))
                    continue;

                clientEvent = map.Translate(message);
                return true;
            }

            clientEvent = null;
            return false;
        }

        private class MappersCollection : IEnumerable<ILogMessageToClientEventTranslation>
        {
            private readonly List<ILogMessageToClientEventTranslation> _logMessageMappers = new List<ILogMessageToClientEventTranslation>();

            public void Add<T>()
                where T : ILogMessageToClientEventTranslation, new()
            {
                _logMessageMappers.Add(new T());
            }

            public IEnumerator<ILogMessageToClientEventTranslation> GetEnumerator()
            {
                return _logMessageMappers.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

    }
}