
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using StatLight.Client.Silverlight.Tests.Mocks;

namespace StatLight.Client.Silverlight.Tests.LogMessageHandling
{
	public class when_the_LogMessages_is_a_ScenarioResult : FixtureBase
	{
		LogMessage _logMessage;
		protected LogMessage LogMessage { get { return _logMessage; } }
		protected override void Before_each_test()
		{
			base.Before_each_test();

			var mockTestMethod = new MockTestMethod();
			var mockTestClass = new MockTestClass();

			ScenarioResult scenarioResult = new ScenarioResult(
				mockTestMethod,
				mockTestClass,
				TestOutcome.Passed,
				null);

			_logMessage = new LogMessage(LogMessageType.TestResult);
			_logMessage.Decorators.Add(UnitTestLogDecorator.IsUnitTestMessage, true);
			_logMessage.Decorators.Add(LogDecorator.NameProperty, "some_test_method_name_here");
			_logMessage.Decorators.Add(LogDecorator.TestGranularity, TestGranularity.TestScenario);
			_logMessage.Decorators.Add(UnitTestLogDecorator.ScenarioResult, scenarioResult);
			_logMessage.Decorators.Add(UnitTestLogDecorator.TestMethodMetadata, mockTestMethod);
			_logMessage.Decorators.Add(UnitTestLogDecorator.TestClassMetadata, mockTestClass);
			_logMessage.Decorators.Add(LogDecorator.TestOutcome, TestOutcome.Passed);
		}
	}
}
