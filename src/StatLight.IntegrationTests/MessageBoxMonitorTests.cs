using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;
using StatLight.Core.Reporting;
using System.Diagnostics;

namespace StatLight.IntegrationTests
{
    [TestFixture]
    public class when_something_executing_in_silverlight_throws_up_a_modal_MessageBox
        : IntegrationFixtureBase
    {
        private ClientTestRunConfiguration clientTestRunConfiguration;
        private TestReport _testReport;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            const string prefix = "StatLight.IntegrationTests.Silverlight.When_a_modal_MessageBox_is_displayed";
            clientTestRunConfiguration = new ClientTestRunConfiguration
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

        [Test]
        public void Should_have_set_the_Namespace_of_messagebox_failed_methods()
        {
            foreach (var result in _testReport.TestResults)
            {
                result.NamespaceName.ShouldEqual("StatLight.IntegrationTests.Silverlight");
            }
        }

        [Test]
        public void Should_only_have_three_results_total()
        {
            _testReport.TestResults.Count().ShouldEqual(3);
        }

        //[Test]
        //public void Should_have_set_the_ClassName_of_messagebox_failed_methods()
        //{
        //    _testReport.TestResults.Select(s => s.NamespaceName.ShouldEqual("StatLight.IntegrationTests.Silverlight"));
        //}
    }
}


