
using System;
using StatLight.Core.Events;

namespace StatLight.Core.Tests.Reporting
{
    using NUnit.Framework;
    using StatLight.Core.Reporting;

    public static class TestCaseResultFactory
    {
        private static DateTime getStartTime()
        {
            return DateTime.Now;
        }
        private static DateTime getEndTime(DateTime startTime)
        {
            return startTime.AddSeconds(1);
        }

        public static TestCaseResult CreatePassed()
        {
            var started = getStartTime();

            return new TestCaseResult(ResultType.Passed)
                        {
                            NamespaceName = "N1",
                            ClassName = "C1",
                            MethodName = "MethodPassed",
                            Started = started,
                            Finished = getEndTime(started),
                        };
        }

        public static TestCaseResult CreateFailed()
        {
            var started = getStartTime();
            return new TestCaseResult(ResultType.Failed)
                        {
                            NamespaceName = "N1",
                            ClassName = "C1",
                            MethodName = "MethodFailed",
                            ExceptionInfo = new Exception(),
                            Started = started,
                            Finished = getEndTime(started),
                        };
        }

        public static TestCaseResult CreateIgnored()
        {
            var started = getStartTime();
            return new TestCaseResult(ResultType.Ignored)
                        {
                            NamespaceName = "N1",
                            ClassName = "C1",
                            MethodName = "MethodIgnored",
                            Started = started,
                            Finished = getEndTime(started),
                        };
        }
    }


    [TestFixture]
    public class when_a_TestReport_is_created : FixtureBase
    {
        [Test]
        public void Should_be_able_to_add_test_results()
        {
            var result = new TestReport("test");

            result.AddResult(TestCaseResultFactory.CreatePassed());
            result.AddResult(TestCaseResultFactory.CreateFailed());

            result.TotalResults.ShouldEqual(2);
        }
    }

    [TestFixture]
    public class when_viewing_a_TestReport_for_a_specific_set_of_results : FixtureBase
    {
        TestReport _result;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            _result = new TestReport("test");

            _result
                .AddResult(TestCaseResultFactory.CreateIgnored())
                .AddResult(TestCaseResultFactory.CreateFailed())
                .AddResult(TestCaseResultFactory.CreateFailed())
                .AddResult(TestCaseResultFactory.CreatePassed())
                .AddResult(TestCaseResultFactory.CreatePassed())
                .AddResult(TestCaseResultFactory.CreatePassed())
                ;
        }

        [Test]
        public void Should_sum_total_time_correctly()
        {
            _result.TimeToComplete.Seconds.ShouldEqual(_result.TotalResults * 1);
        }

        [Test]
        public void Should_count_total_correctly()
        {
            _result.TotalResults.ShouldEqual(6);
        }

        [Test]
        public void can_get_count_of_failing_tests()
        {
            _result.TotalFailed.ShouldEqual(2);
        }

        [Test]
        public void can_get_count_of_passing_tests()
        {
            _result.TotalPassed.ShouldEqual(3);
        }

        [Test]
        public void the_final_result_should_be_failed()
        {
            _result.FinalResult.ShouldEqual(RunCompletedState.Failure);
        }


        [Test]
        public void when_only_passing_tests_are_added_the_FinalResult_should_be_successful()
        {
            var result = new TestReport("test");

            result
                .AddResult(TestCaseResultFactory.CreatePassed())
                .AddResult(TestCaseResultFactory.CreatePassed());

            result.FinalResult.ShouldEqual(RunCompletedState.Successful);
        }

    }
}
