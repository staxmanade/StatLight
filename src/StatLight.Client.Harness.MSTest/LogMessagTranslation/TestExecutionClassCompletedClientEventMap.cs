using Microsoft.Silverlight.Testing.Harness;
#if MSTest2009July || MSTest2009October || MSTest2009November
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#else
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Core.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
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
                                       ClassName = testClass.Type.ClassNameIncludingParentsIfNested(),
                                       NamespaceName = testClass.Type.Namespace,
                                   };
            return clientEventX;
        }

    }
}