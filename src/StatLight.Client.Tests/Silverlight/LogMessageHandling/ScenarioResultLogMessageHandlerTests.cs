using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing;
using StatLight.Client.Tests;

namespace StatLight.Client.Silverlight.LogMessageHandling
{
    [TestClass]
    [Tag("ScenarioResult")]
    public class for_a_ScenarioResultLogMessageHandler : when_the_LogMessages_is_a_ScenarioResult
    {
        ILogMessageHandler logMessageHandler;
        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            logMessageHandler = LogMessageHandlerFactory.GetHandlerFor(LogMessage);
        }

        [TestMethod]
        public void should_be_able_to_serialize_the_logMessage()
        {
            logMessageHandler.Serialize()
                .ShouldNotBeNull();
        }


    }
}


