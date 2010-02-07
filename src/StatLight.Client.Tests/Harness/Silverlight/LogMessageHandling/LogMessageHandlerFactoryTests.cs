using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing;
using StatLight.Client.Tests;

namespace StatLight.Client.Harness.LogMessageHandling
{
    [TestClass]
    [Tag("LogMessage")]
    public class when_retrieving_a_LogMessageHandler_from_the_inspector_factory : when_the_LogMessages_is_a_ScenarioResult
    {
        ILogMessageHandler logMessageHandler;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            logMessageHandler = LogMessageHandlerFactory.GetHandlerFor(LogMessage);
        }

        [TestMethod]
        public void should_find_a_handler_for_the_ScenarioResult_log_message()
        {
            logMessageHandler.ShouldNotBeNull();
        }

        [TestMethod]
        public void the_found_handler_should_be_an_instance_if_ScenarioResultLogMessageHandler()
        {
            logMessageHandler.ShouldBeInstanceOfType(typeof(ScenarioResultLogMessageHandler));
        }

        [TestMethod]
        [Ignore]
        public void IgnoredTest()
        {
            Assert.Fail("This test should not run");
        }
    }

    //[TestClass]
    //public class when_getting_the_LogMessageReaderResult_for_a_ScenarioResultX : when_dealing_with_a_ScenarioResult_log_message
    //{
    //    MobilLogMessage result;

    //    protected override void Before_all_tests()
    //    {
    //        base.Before_all_tests();

    //        LogMessageReaderFactory.Register(LogMessageType.TestResult, new ScenarioResultLogMessageReader());
    //        ILogMessageReader reader = LogMessageReaderFactory.GetLogMessageReaderFor(LogMessage);
    //        MobilLogMessage result = reader.Read(LogMessage);
    //    }

    //    [TestMethod]
    //    public void the_LogMessageType_should_be_the_TestResult()
    //    {
    //        result.LogMessageType.ShouldBeEqualTo(LogMessageType.TestResult);
    //    }

    //}
}


