using Microsoft.Silverlight.Testing.Harness;
#if March2010 || April2010 || May2010
#else
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
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
            clientEventX.AssignTestExecutionMethodInfo(testMethod);
            return clientEventX;
        }
    }
}