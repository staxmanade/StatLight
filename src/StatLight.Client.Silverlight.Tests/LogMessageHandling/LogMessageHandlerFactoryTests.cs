using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using System.Collections.Generic;
using StatLight.Core;
using StatLight.Client.Silverlight.LogMessageHandling;
using Microsoft.Silverlight.Testing;

namespace StatLight.Client.Silverlight.Tests.LogMessageHandling
{
	[TestClass]
	[Tag("LogMessage")]
	public class when_retrieving_a_LogMessageHandler_from_the_inspector_factory : when_the_LogMessages_is_a_ScenarioResult
	{
		ILogMessageHandler logMessageHandler;

		protected override void Before_each_test()
		{
			base.Before_each_test();

			logMessageHandler = LogMessageHandlerFactory.GetHandlerFor(base.LogMessage);
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

	//    protected override void Before_each_test()
	//    {
	//        base.Before_each_test();

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
