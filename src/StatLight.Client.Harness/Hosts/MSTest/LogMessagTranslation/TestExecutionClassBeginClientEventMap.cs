using Microsoft.Silverlight.Testing.Harness;
#if MSTestMarch2010
#else
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public class TestExecutionClassBeginClientEventMap : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {
            if (message.MessageType == LogMessageType.TestExecution)
            {
                if (message.Is(TestStage.Starting)
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
            var clientEventX = new TestExecutionClassBeginClientEvent
                                   {
                                       ClassName = testClass.Type.ReadClassName(),
                                       NamespaceName = testClass.Type.Namespace,
                                   };
            return clientEventX;
        }
    }
}