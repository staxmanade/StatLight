using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using StatLight.Core.Reporting.Messages;
using StatLight.Core.Serialization;

namespace StatLight.Client.Harness.LogMessageHandling
{
	internal class ScenarioResultLogMessageHandler : ILogMessageHandler
	{
		private LogMessage logMessage;
		private MobilScenarioResult mobilScenarioResult;
		public ScenarioResultLogMessageHandler(LogMessage logMessage)
		{
			this.logMessage = logMessage;

			var scenarioResult = logMessage.Decorators[UnitTestLogDecorator.ScenarioResult] as ScenarioResult;

			mobilScenarioResult = new MobilScenarioResult();
			mobilScenarioResult.ExceptionMessage = scenarioResult.Exception != null ? scenarioResult.Exception.ToString() : string.Empty;
			mobilScenarioResult.Finished = scenarioResult.Finished;
			mobilScenarioResult.Result = (StatLight.Core.Reporting.Messages.TestOutcome)scenarioResult.Result;
			mobilScenarioResult.Started = scenarioResult.Started;
			mobilScenarioResult.TestClassName = scenarioResult.TestClass.Name;
			mobilScenarioResult.TestName = scenarioResult.TestMethod.Name;

		}

		public string Serialize()
		{
			return mobilScenarioResult.Serialize();
		}
	}
}
