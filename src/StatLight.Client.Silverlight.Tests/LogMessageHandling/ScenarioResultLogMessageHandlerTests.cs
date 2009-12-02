using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Silverlight.LogMessageHandling;
using Microsoft.Silverlight.Testing;

namespace StatLight.Client.Silverlight.Tests.LogMessageHandling
{
	[TestClass]
	[Tag("ScenarioResult")]
	public class for_a_ScenarioResultLogMessageHandler : when_the_LogMessages_is_a_ScenarioResult
	{
		ILogMessageHandler logMessageHandler;
		protected override void Before_each_test()
		{
			base.Before_each_test();

			logMessageHandler = LogMessageHandlerFactory.GetHandlerFor(base.LogMessage);
		}

		[TestMethod]
		public void should_be_able_to_serialize_the_logMessage()
		{
			logMessageHandler.Serialize()
				.ShouldNotBeNull();
		}


	}
}
