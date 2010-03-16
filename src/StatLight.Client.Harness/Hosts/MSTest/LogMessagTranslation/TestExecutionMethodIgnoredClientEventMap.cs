using System;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public class TestExecutionMethodIgnoredClientEventMap : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {

            if (message.MessageType == LogMessageType.TestExecution)
            {
                if (message.Is(TestGranularity.TestScenario)
                    && message.DecoratorMatches(UnitTestLogDecorator.IgnoreMessage, v => (bool)v)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        public ClientEvent Translate(LogMessage message)
        {
            var testName = (string)message.Decorators[LogDecorator.NameProperty];
            var clientEventX = new TestExecutionMethodIgnoredClientEvent
                                   {
                                       ClassName = null,
                                       NamespaceName = null,
                                       MethodName = testName,
                                       Started = DateTime.Now,
                                   };

            return clientEventX;
        }
    }
}