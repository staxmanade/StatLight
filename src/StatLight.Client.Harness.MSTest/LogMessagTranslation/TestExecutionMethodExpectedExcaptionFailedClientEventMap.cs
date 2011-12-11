using System;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
#if MSTest2009July || MSTest2009October || MSTest2009November
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#else
#endif
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public class TestExecutionMethodExpectedExcaptionFailedClientEventMap : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {
            if (message.MessageType == LogMessageType.TestResult)
            {
                if (message.Is(TestGranularity.TestScenario)
                    && message.DecoratorMatches(LogDecorator.TestOutcome, v =>
                                                                              {
                                                                                  switch ((TestOutcome)v)
                                                                                  {
                                                                                      case TestOutcome.Failed:
                                                                                          return true;
                                                                                      default:
                                                                                          return false;
                                                                                  }
                                                                              })
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
            var clientEventX = new TestExecutionMethodFailedClientEvent
                                   {
                                       ExceptionInfo = new ExceptionInfo(new Exception("An expected exception was not thrown.")),
                                       Finished = scenarioResult.Finished,
                                       Started = scenarioResult.Started,
                                   };
            clientEventX.AssignMetadata(testMethod.Method);
            clientEventX.AssignTestExecutionMethodInfo(testMethod);

            return clientEventX;
        }
    }
}