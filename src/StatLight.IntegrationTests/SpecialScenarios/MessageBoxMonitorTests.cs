using System.Linq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Tests;
using StatLight.Core.Reporting;

namespace StatLight.IntegrationTests.SpecialScenarios
{
    [TestFixture]
    public class when_something_executing_in_silverlight_throws_up_a_modal_MessageBox
        : SpecialScenariosBase
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get
            {
                if (_clientTestRunConfiguration == null)
                {
                    const string prefix = "StatLight.IntegrationTests.Silverlight.When_a_modal_MessageBox_is_displayed";
                    _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(
                        new []
                        {
                            prefix + ".messageBox_overload_1X",
                            prefix + ".messageBox_overload_1_MessageBoxButton_OKX",
                            prefix + ".messageBox_overload_1_MessageBoxButton_OKCancel",
                        });
                }

                return _clientTestRunConfiguration;
            }
        }

        [Test]
        public void the_final_result_should_be_a_failure()
        {
            TestReport.FinalResult.ShouldEqual(RunCompletedState.Failure);
        }

        [Test]
        public void Should_have_detected_three_message_box_failures()
        {
            TestReport.TotalFailed.ShouldEqual(3);
        }

        [Test]
        [Ignore("Can't currently do as we can't figure out which method the message box was thrown up in")]
        public void Should_have_set_the_Namespace_of_messagebox_failed_methods()
        {
            foreach (var result in TestReport.TestResults)
            {
                result.NamespaceName.ShouldEqual("StatLight.IntegrationTests.Silverlight");
            }
        }

        [Test]
        public void Should_only_have_three_results_total_EXCEPT_cant_detect_which_method_to_map_the_message_box_to_so_our_total_is_now_six()
        {
            TestReport.TestResults.Count().ShouldEqual(6);
        }

        [Test]
        public void Should_have_scraped_the__messageBox_overload_1__test_message_box_info()
        {
            TestReport
                .TestResults
                .Where(w => !string.IsNullOrEmpty(w.OtherInfo))
                .Single(w => w.OtherInfo.Contains("messageBox_overload_1X"))
                .OtherInfo
                .ShouldContain("Some text");
        }

        [Test]
        [Ignore]
        public void Should_have_scraped_the__messageBox_overload_1_MessageBoxButton_OK__test_message_box_info()
        {
            TestReport
                .TestResults
                // FYI: This just asserts that some message was scraped - not that any specific method contains the message box info
                .Where(w => !string.IsNullOrEmpty(w.OtherInfo))
                .Single(w => w.OtherInfo.Contains("messageBox_overload_1_MessageBoxButton_OKX"))
                .OtherInfo
                .ShouldContain("some caption")
                .ShouldContain("Some text");
        }

        [Test]
        public void Should_have_scraped_the__messageBox_overload_1_MessageBoxButton_OKCancel__test_message_box_info()
        {
            TestReport
                .TestResults
                .Where(w => !string.IsNullOrEmpty(w.OtherInfo))
                .Single(w => w.OtherInfo.Contains("messageBox_overload_1_MessageBoxButton_OKCancel"))
                .OtherInfo
                .ShouldContain("some caption")
                .ShouldContain("Some text");
        }
    }
}


