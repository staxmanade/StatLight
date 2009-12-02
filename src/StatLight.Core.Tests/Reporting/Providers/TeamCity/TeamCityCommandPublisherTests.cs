namespace StatLight.Core.Tests.Reporting.Providers.TeamCity
{
	using System.Collections.Generic;
	using System.Linq;
	using NUnit.Framework;
	using StatLight.Core.Reporting.Messages;
	using StatLight.Core.Reporting.Providers.TeamCity;
	using StatLight.Core.Tests.Mocks;

	class TestMessageWriter : ICommandWriter
	{
		private readonly List<string> messages = new List<string>();
		public List<string> Messages { get { return messages; } }

		public void Write(Command command)
		{
			this.messages.Add(command.ToString());
		}
	}

	[TestFixture]
	public class when_reporting_a_test_run_through_the_TeamCityCommandPublisher : FixtureBase
	{
		TeamCityTestResultHandler publisher;
		TestMessageWriter writer;

		protected override void Before_each_test()
		{
			base.Before_each_test();

			writer = new TestMessageWriter();

			publisher = new TeamCityTestResultHandler(writer, "assemblyName.here");
		}

		[Test]
		public void when_we_start_the_publishing_it_should_write_the_testSuiteStarted_message_first()
		{
			publisher.PublishStart();

			writer.Messages.First()
				.ShouldContain(CommandType.testSuiteStarted.ToString());
		}

		[Test]
		public void when_we_stop_the_publishing_it_should_write_the_testSuiteFinished_message_last()
		{
			publisher.PublishStop();

			writer.Messages.Last()
				.ShouldContain(CommandType.testSuiteFinished.ToString());
		}

		[Test]
		public void when_given_a_passing_test_it_should_write_the_correct_begin_and_end_messages()
		{
			var message = MessageFactory.CreateResult(TestOutcome.Passed);
			publisher.HandleMessage(message);

			writer.Messages
				.Where(w => w.Contains(CommandType.testStarted.ToString()))
				.Count().ShouldEqual(1);

			writer.Messages
				.Where(w => w.Contains(CommandType.testFinished.ToString()))
				.Count().ShouldEqual(1);
		}


		[Test]
		public void when_given_a_failing_test_it_should_write_the_correct_message_begin_end_and_failure_messages()
		{
			var message = MessageFactory.CreateResult(TestOutcome.Failed);
			publisher.HandleMessage(message);

			writer.Messages
				.Where(w => w.Contains(CommandType.testStarted.ToString()))
				.Count().ShouldEqual(1);

			writer.Messages
				.Where(w => w.Contains(CommandType.testFailed.ToString()))
				.Count().ShouldEqual(1);

			writer.Messages
				.Where(w => w.Contains(CommandType.testFinished.ToString()))
				.Count().ShouldEqual(1);
		}

		[Test]
		public void when_given_an_ignore_test_it_should_write_the_correct_message_begin_end_and_ignore_messages()
		{
			string testNameIgnored = "some_test_to_ignore";
			var message = MessageFactory.CreateTestIgnoreMessage(testNameIgnored);

			publisher.HandleMessage(message);

			writer.Messages
				.Where(w => w.Contains(CommandType.testStarted.ToString()))
				.Count().ShouldEqual(1);

			writer.Messages
				.Where(w => w.Contains(CommandType.testIgnored.ToString()))
				.Count().ShouldEqual(1);

			writer.Messages
				.Where(w => w.Contains(CommandType.testFinished.ToString()))
				.Count().ShouldEqual(1);
		}
	}
}
