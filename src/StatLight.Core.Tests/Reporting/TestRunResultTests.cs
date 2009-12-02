
using StatLight.Core.Reporting.Messages;

namespace StatLight.Core.Tests.Reporting
{
	using NUnit.Framework;
	using StatLight.Core.Reporting;
	using StatLight.Core.Tests.Mocks;
	using StatLight.Core.Runners;

	[TestFixture]
	public class when_a_TestReport_is_created : FixtureBase
	{
		[Test]
		public void Should_be_able_to_add_test_results()
		{
			var result = new TestReport();

			result.AddResult(MessageFactory.CreateResult(TestOutcome.Passed));
			result.AddResult(MessageFactory.CreateResult(TestOutcome.Failed));

			result.TotalResults.ShouldEqual(2);
		}
	}

	[TestFixture]
	public class when_viewing_a_TestReport_for_a_specific_set_of_results : FixtureBase
	{
		TestReport _result;
		protected override void Before_each_test()
		{
			base.Before_each_test();

			_result = new TestReport();

			_result
				.AddResult(MessageFactory.CreateResult(TestOutcome.Passed))
				.AddResult(MessageFactory.CreateResult(TestOutcome.Passed))
				.AddResult(MessageFactory.CreateResult(TestOutcome.Failed));
		}

		[Test]
		public void can_get_count_of_failing_tests()
		{
			_result.TotalFailed.ShouldEqual(1);
		}

		[Test]
		public void can_get_count_of_passing_tests()
		{
			_result.TotalPassed.ShouldEqual(2);
		}

		[Test]
		public void the_final_result_should_be_failed()
		{
			_result.FinalResult.ShouldEqual(RunCompletedState.Failure);
		}


		[Test]
		public void when_only_passing_tests_are_added_the_FinalResult_should_be_successful()
		{
			var result = new TestReport();

			result
				.AddResult(MessageFactory.CreateResult(TestOutcome.Passed))
				.AddResult(MessageFactory.CreateResult(TestOutcome.Passed));

			result.FinalResult.ShouldEqual(RunCompletedState.Successful);
		}

	}
}
