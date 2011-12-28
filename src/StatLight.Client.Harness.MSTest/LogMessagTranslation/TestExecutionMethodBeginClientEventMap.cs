using System;
using Microsoft.Silverlight.Testing.Harness;
#if MSTest2009July || MSTest2009October || MSTest2009November
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#else
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Core.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
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
                                       Started = DateTime.Now,
                                   };
            clientEventX.AssignTestExecutionMethodInfo(testMethod);
            return clientEventX;
        }
    }
}