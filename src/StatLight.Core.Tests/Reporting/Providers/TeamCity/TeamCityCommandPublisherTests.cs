namespace StatLight.Core.Tests.Reporting.Providers.TeamCity
{
	using System.Collections.Generic;
	using System.Linq;
	using NUnit.Framework;
	using StatLight.Core.Reporting.Messages;
	using StatLight.Core.Reporting.Providers.TeamCity;
	using StatLight.Core.Tests.Mocks;

	public class TestMessageWriter : ICommandWriter
	{
		private readonly List<string> messages = new List<string>();
		public List<string> Messages { get { return messages; } }

		public void Write(Command command)
		{
			this.messages.Add(command.ToString());
		}
	}

	public class when_reporting_a_test_run_through_the_TeamCityCommandPublisher : FixtureBase
	{
		internal protected TeamCityTestResultHandler publisher;
		internal protected TestMessageWriter writer;

		protected override void Before_each_test()
		{
			base.Before_each_test();

			writer = new TestMessageWriter();

			publisher = new TeamCityTestResultHandler(writer, "assemblyName.here");
		}
	}

	[TestFixture]
	public class when_publishing_the_start_command :
		when_reporting_a_test_run_through_the_TeamCityCommandPublisher
	{

		protected override void Because()
		{
			base.Because();
			publisher.PublishStart();
		}

		[Test]
		public void when_we_start_the_publishing_it_should_write_the_testSuiteStarted_message_first()
		{
			writer.Messages.First()
				.ShouldContain(CommandType.testSuiteStarted.ToString());
		}
	}


	[TestFixture]
	public class when_publishing_the_stop_command :
		when_reporting_a_test_run_through_the_TeamCityCommandPublisher
	{

		protected override void Because()
		{
			base.Because();
			publisher.PublishStop();
		}

		[Test]
		public void when_we_stop_the_publishing_it_should_write_the_testSuiteFinished_message_last()
		{
			writer.Messages.Last()
				.ShouldContain(CommandType.testSuiteFinished.ToString());
		}
	}


	[TestFixture]
	public class for_a_passed_test_outcome :
		when_reporting_a_test_run_through_the_TeamCityCommandPublisher
	{
		protected override void Because()
		{
			base.Because();
			var message = MessageFactory.CreateResult(TestOutcome.Passed);
			publisher.HandleMessage(message);
		}

		[Test]
		public void Should_write_the_correct_begin_message()
		{
			writer.Messages
				.Where(w => w.Contains(CommandType.testStarted.ToString()))
				.Count().ShouldEqual(1);
		}

		[Test]
		public void Should_write_the_correct_end_message()
		{
			writer.Messages
				.Where(w => w.Contains(CommandType.testFinished.ToString()))
				.Count().ShouldEqual(1);
		}
	}


	[TestFixture]
	public class for_a_failed_test_outcome :
		when_reporting_a_test_run_through_the_TeamCityCommandPublisher
	{

		protected override void Because()
		{
			base.Because();
			var message = MessageFactory.CreateResult(TestOutcome.Failed);
			publisher.HandleMessage(message);
		}

		[Test]
		public void Should_write_the_begin_message()
		{
			writer.Messages
				.Where(w => w.Contains(CommandType.testStarted.ToString()))
				.Count().ShouldEqual(1);
		}

		[Test]
		public void Should_write_the_failure_message()
		{
			writer.Messages
				.Where(w => w.Contains(CommandType.testFailed.ToString()))
				.Count().ShouldEqual(1);
		}

		[Test]
		public void Should_write_the_end_message()
		{
			writer.Messages
					.Where(w => w.Contains(CommandType.testFinished.ToString()))
					.Count().ShouldEqual(1);
		}
	}

	[TestFixture]
	public class for_an_ignored_test_outcome :
		when_reporting_a_test_run_through_the_TeamCityCommandPublisher
	{

		protected override void Because()
		{
			base.Because();
			string testNameIgnored = "some_test_to_ignore";
			var message = MessageFactory.CreateTestIgnoreMessage(testNameIgnored);
			publisher.HandleMessage(message);
		}

		[Test]
		public void Should_write_the_begin_message()
		{
			writer.Messages
				.Where(w => w.Contains(CommandType.testStarted.ToString()))
				.Count().ShouldEqual(1);
		}

		[Test]
		public void Should_write_the_ignore_message()
		{
			writer.Messages
				.Where(w => w.Contains(CommandType.testIgnored.ToString()))
				.Count().ShouldEqual(1);
		}

		[Test]
		public void Should_write_the_end_message()
		{
			writer.Messages
					.Where(w => w.Contains(CommandType.testFinished.ToString()))
					.Count().ShouldEqual(1);
		}
	}
}
