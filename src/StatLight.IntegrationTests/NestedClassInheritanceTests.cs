using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Reporting;
using StatLight.Core.Tests;

namespace StatLight.IntegrationTests
{
    [TestFixture]
    public class when_something_tests_are_in_a_NestedCLassInheritance_structure
        : IntegrationFixtureBase
    {
        private ClientTestRunConfiguration clientTestRunConfiguration;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            const string prefix = "StatLight.IntegrationTests.Silverlight.NestedClassInheritanceTests.A+B";
            clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(
                new List<string>
                   {
                       prefix + "1.MethodA",
                       prefix + "2.MethodA",
                   });
        }

        [Test]
        public void the_final_result_should_be_a_failure()
        {
            TestReport.FinalResult.ShouldEqual(RunCompletedState.Successful);
        }

        [Test]
        public void Should_have_detected_three_message_box_failures()
        {
            TestReport.TotalFailed.ShouldEqual(0);
        }

        [Test]
        public void Should_only_have_two_results_because_this_should_have_only_executed_2_tests()
        {
            TestReport.TestResults.Count().ShouldEqual(2);
        }
    }
}