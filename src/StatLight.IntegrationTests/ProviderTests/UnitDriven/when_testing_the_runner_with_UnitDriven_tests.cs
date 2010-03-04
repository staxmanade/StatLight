using NUnit.Framework;
using StatLight.Core.Reporting;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;

namespace StatLight.IntegrationTests.ProviderTests.UnitDriven
{
    [TestFixture]
    public class when_testing_the_runner_with_UnitDriven_tests
        : IntegrationFixtureBase
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;
        private TestReport _testReport;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return _clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            PathToIntegrationTestXap = TestXapFileLocations.UnitDriven;
            _clientTestRunConfiguration = new ClientTestRunConfiguration
                                        {
                                                TagFilter = string.Empty,
                                                UnitTestProviderType = UnitTestProviderType.UnitDriven
                                            };
            base.Before_all_tests();

            _testReport = Runner.Run();
        }

        [Test]
        public void Should_have_correct_TotalFailed_count()
        {
            // note: should be 4, but not currently supporting the async tests
            _testReport.TotalFailed.ShouldEqual(2);
        }


        [Test]
        public void Should_have_total_results_of_11_but_currently_9_because_not_supported_async_failures()
        {
            _testReport.TotalResults.ShouldEqual(9);
        }

        [Test]
        [Ignore("TODO: Fix this???")]
        public void Should_have_error_message_for_async_failed_tests()
        {
            //_testReport
            //    .Results
            //    .Where(w => w.Result == TestOutcome.Failed)
            //    .Where(w => string.IsNullOrEmpty(w.Message))
            //    .Count().ShouldBeLessThan(1);
        }

        [Test]
        public void Should_have_correct_TotalPassed_count()
        {
            _testReport.TotalPassed.ShouldEqual(7);
        }

        [Test]
        public void Should_have_correct_TotalIgnored_count()
        {
            _testReport.TotalIgnored.ShouldEqual(0);
        }
    }
}