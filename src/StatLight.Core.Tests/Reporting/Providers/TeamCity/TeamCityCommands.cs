namespace StatLight.Core.Tests.Reporting.Providers.TeamCity
{
	using NUnit.Framework;
	using StatLight.Core.Reporting.Providers.TeamCity;

	[TestFixture]
	public class when_creating_a_team_city_command : FixtureBase
	{
		Command command;
		protected override void Before_all_tests()
		{
			base.Before_all_tests();

			command = CommandFactory.TestSuiteStarted("asemblyName");
		}

		[Test]
		public void should_be_able_to_create_a_team_city_command()
		{
			command.ShouldNotBeNull();
		}

		[Test]
		public void the_command_text_should_start_with_speced_TeamCity_start_command()
		{
			command.ToString().ShouldStartWith("##teamcity[");
		}

		[Test]
		public void the_command_text_should_end_with_speced_TeamCity_end_bracket()
		{
			command.ToString().ShouldEndWith("]");
		}
	}

	[TestFixture]
	public class when_verifying_protocol_for_teamcity_commands : FixtureBase
	{
		string assemblyName = "test.assembly";

		[Test]
		public void should_be_able_to_create_a_testSuiteStarted_command()
		{
			var command = CommandFactory.TestSuiteStarted(assemblyName);
			command.ToString().ShouldEqual(wrapMessage("testSuiteStarted name='" + assemblyName + "'"));
		}

		[Test]
		public void should_be_able_to_create_a_testSuiteFinished_command()
		{
			var command = CommandFactory.TestSuiteFinished(assemblyName);
			command.ToString().ShouldEqual(wrapMessage("testSuiteFinished name='" + assemblyName + "'"));
		}

		[Test]
		public void should_be_able_to_create_a_testStarted_command_and_the_captureStandardOutput_it_true()
		{
			var command = CommandFactory.TestStarted(assemblyName);
			command.ToString().ShouldEqual(wrapMessage("testStarted name='" + assemblyName + "' captureStandardOutput='true'"));
		}

		[Test]
		public void should_be_able_to_create_a_testFinished_command()
		{
			long duration = 1000;
			var command = CommandFactory.TestFinished(assemblyName, duration);
			command.ToString().ShouldEqual(wrapMessage("testFinished name='" + assemblyName + "' duration='1000'"));
		}

		[Test]
		public void should_be_able_to_create_a_testIgnored_command()
		{
			var reason = "this test was ignored";
			var command = CommandFactory.TestIgnored(assemblyName, reason);
			command.ToString().ShouldEqual(wrapMessage("testIgnored name='" + assemblyName + "' message='" + reason + "'"));
		}


		[Test]
		public void should_be_able_to_create_a_testFailed_command()
		{
			var message = "failed test message";
			var details = "details of the failed test message";

			var command = CommandFactory.TestFailed(assemblyName, message, details);
			command.ToString().ShouldEqual(wrapMessage("testFailed name='" + assemblyName + "' message='" + message + "' details='" + details + "'"));
		}

		[Test]
		public void should_be_able_to_create_a_testFailed_with_type_comparison_command()
		{
			var message = "failed test message";
			var details = "details of the failed test message";
			var type = "comparisonFailure";
			var expected = "expected value";
			var actual = "actual value";

			var command = CommandFactory.TestFailed(assemblyName, message, details, type, expected, actual);
			command.ToString().ShouldEqual(wrapMessage("testFailed type='" + type + "' name='" + assemblyName + "' message='" + message + "' details='" + details + "' expected='" + expected + "' actual='" + actual + "'"));
		}

		[Test]
		public void should_be_able_to_create_a_testStdOut_command()
		{
			var @out = "some out text";
			var command = CommandFactory.TestStdOut(assemblyName, @out);
			command.ToString().ShouldEqual(wrapMessage("testStdOut name='" + assemblyName + "' out='" + @out + "'"));
		}

		[Test]
		public void should_be_able_to_create_a_testStdErr_command()
		{
			var @out = "some out text";
			var command = CommandFactory.TestStdErr(assemblyName, @out);
			command.ToString().ShouldEqual(wrapMessage("testStdErr name='" + assemblyName + "' out='" + @out + "'"));
		}

		private string wrapMessage(string msg)
		{
			return "##teamcity[" + msg + "]";
		}
	}
}
