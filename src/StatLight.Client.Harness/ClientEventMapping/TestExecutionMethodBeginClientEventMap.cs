using System;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Client.Model.Events;

namespace StatLight.Client.Harness.ClientEventMapping
{
    public class TestExecutionMethodBeginClientEventMap : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {
            if (message.MessageType == LogMessageType.TestExecution)
            {
                if (message.Is(TestStage.Starting)
                    && message.Is(TestGranularity.TestScenario)
                    && message.DecoratorMatches(UnitTestLogDecorator.TestMethodMetadata, v => v is ITestMethod)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        public ClientEvent Translate(LogMessage message)
        {
            var testMethod = (ITestMethod)message.Decorators[UnitTestLogDecorator.TestMethodMetadata];
            var clientEventX = new TestExecutionMethodBeginClientEvent
                                   {
                                       ClassName = testMethod.Method.DeclaringType.Name,
                                       NamespaceName = testMethod.Method.DeclaringType.Namespace,
                                       MethodName = testMethod.Method.Name,
                                       Started = DateTime.Now,
                                   };
            return clientEventX;
        }
    }
}