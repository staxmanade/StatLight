using Microsoft.Silverlight.Testing.Harness;
#if MSTest2009July || MSTest2009October || MSTest2009November
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#else 
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Core.Events;

namespace StatLight.Core.Events.Hosts.MSTest.LogMessagTranslation
{
    public class TestExecutionMethodPassedClientEventMap : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {
            if (message.MessageType == LogMessageType.TestResult)
            {
                if (message.Is(TestGranularity.TestScenario)
                    && message.DecoratorMatches(LogDecorator.TestOutcome, v => (TestOutcome)v == TestOutcome.Passed)
                    )
                {
                    return true;
                }
            }
            return false;
        }

        public ClientEvent Translate(LogMessage message)
        {
            var scenarioResult = (ScenarioResult)message.Decorators[UnitTestLogDecorator.ScenarioResult];
            var testMethod = (ITestMethod)message.Decorators[UnitTestLogDecorator.TestMethodMetadata];

            var clientEventX = new TestExecutionMethodPassedClientEvent
                                   {
                                       Finished = scenarioResult.Finished,
                                       Started = scenarioResult.Started,
                                   };
            clientEventX.AssignMetadata(testMethod.Method);
            clientEventX.AssignTestExecutionMethodInfo(testMethod);
            return clientEventX;
        }
    }
}