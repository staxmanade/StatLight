using NUnit.Framework;
using StatLight.Core.Reporting.Messages;
using StatLight.Core.Reporting.Providers.Console;

namespace StatLight.Core.Tests.Reporting.Providers.Console
{
	[TestFixture]
	public class when_verifying_the_console_runner_can_accept_messages : FixtureBase
	{
		ConsoleResultHandler handler;

		protected override void Before_all_tests()
		{
			base.Before_all_tests();

			handler = new ConsoleResultHandler();
		}

		[Test]
		public void should_take_a_passing_result()
		{
			handler.HandleMessage(Tests.Mocks.MessageFactory.CreateResult(TestOutcome.Passed));
		}

		[Test]
		public void should_take_a_failing_result()
		{
			handler.HandleMessage(Tests.Mocks.MessageFactory.CreateResult(TestOutcome.Failed));
		}


		[Test]
		public void should_be_able_to_handel_an_other_message_type()
		{
			handler.HandleMessage(Tests.Mocks.MessageFactory.CreateOtherMessageType(LogMessageType.Environment));
		}
	}
}
