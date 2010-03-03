using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.ClientEventMapping
{
    public class TestExecutionMethodFailedClientEventMap : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {
            if (message.MessageType == LogMessageType.TestResult)
            {
                if (message.Is(TestGranularity.TestScenario)
                    && message.DecoratorMatches(LogDecorator.TestOutcome, v =>
                                                                              {
                                                                                  TestOutcome outcome = (TestOutcome)v;
                                                                                  switch ((TestOutcome)v)
                                                                                  {
                                                                                      case TestOutcome.Failed:
                                                                                      case TestOutcome.Inconclusive:
                                                                                          //TODO: reproduce case TestOutcome.Error:
                                                                                          return true;
                                                                                      default:
                                                                                          return false;
                                                                                  }
                                                                              })
                    && message.DecoratorMatches(UnitTestLogDecorator.ScenarioResult, v => ((ScenarioResult)v).Exception != null)
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
            var exception = scenarioResult.Exception;
            var testMethod = (ITestMethod)message.Decorators[UnitTestLogDecorator.TestMethodMetadata];
            var clientEventX = new TestExecutionMethodFailedClientEvent
                                   {
                                       ExceptionInfo = exception,
                                       ClassName = testMethod.Method.DeclaringType.ReadClassName(),
                                       NamespaceName = testMethod.Method.DeclaringType.Namespace,
                                       MethodName = testMethod.Method.Name,
                                       Finished = scenarioResult.Finished,
                                       Started = scenarioResult.Started,
                                   };

            return clientEventX;
        }
    }
}