using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.ClientEventMapping
{
    public class TestExecutionClassCompletedClientEventMap : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {
            if (message.MessageType == LogMessageType.TestExecution)
            {
                if (message.Is(TestStage.Finishing)
                    && message.Is(TestGranularity.Test)
                    && message.DecoratorMatches(UnitTestLogDecorator.TestClassMetadata, v => v is ITestClass)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        public ClientEvent Translate(LogMessage message)
        {
            var testClass = (ITestClass)message.Decorators[UnitTestLogDecorator.TestClassMetadata];
            var clientEventX = new TestExecutionClassCompletedClientEvent
                                   {
                                       ClassName = testClass.Name,
                                       NamespaceName = testClass.Type.Namespace,
                                   };
            return clientEventX;
        }

    }
}