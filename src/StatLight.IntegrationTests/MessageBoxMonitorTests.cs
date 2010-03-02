using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;
using StatLight.Core.Reporting;

namespace StatLight.IntegrationTests
{
    [TestFixture]
    public class when_something_executing_in_silverlight_throws_up_a_modal_MessageBox
        : IntegrationFixtureBase
    {
        private TestRunConfiguration testRunConfiguration;
        private TestReport _testReport;

        protected override TestRunConfiguration TestRunConfiguration
        {
            get { return testRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            const string prefix = "StatLight.IntegrationTests.Silverlight.When_a_modal_MessageBox_is_displayed";
            testRunConfiguration = new TestRunConfiguration
                                       {
                                           TagFilter = string.Empty,
                                           UnitTestProviderType = UnitTestProviderType.MSTest,
                                           MethodsToTest = new List<string>
                                                               {
                                                                   prefix + ".messageBox_overload_1",
                                                                   prefix + ".messageBox_overload_1_MessageBoxButton_OK",
                                                                   prefix + ".messageBox_overload_1_MessageBoxButton_OKCancel",
                                                               }
                                       };

            base.Before_all_tests();
        }

        protected override void Because()
        {
            base.Because();
            _testReport = base.Runner.Run();
        }

        [Test]
        public void the_final_result_should_be_a_failure()
        {
            _testReport.FinalResult.ShouldEqual(RunCompletedState.Failure);
        }

        [Test]
        public void Should_have_detected_three_message_box_failures()
        {
            _testReport.TotalFailed.ShouldEqual(3);
        }
    }
}


