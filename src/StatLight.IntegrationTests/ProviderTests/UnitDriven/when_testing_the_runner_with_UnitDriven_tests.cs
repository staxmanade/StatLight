using System.Linq;
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

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return _clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();
            PathToIntegrationTestXap = TestXapFileLocations.UnitDriven;
            _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(UnitTestProviderType.UnitDriven);
        }

        [Test]
        public void Should_have_correct_TotalFailed_count()
        {
            TestReport.TotalFailed.ShouldEqual(4);
        }


        [Test]
        public void Should_have_correct_number_of_TotalResults()
        {
            TestReport.TotalResults.ShouldEqual(11);
        }

        [Test]
        public void Should_have_correct_TotalPassed_count()
        {
            TestReport.TotalPassed.ShouldEqual(7);
        }

        [Test]
        public void Should_have_correct_TotalIgnored_count()
        {
            TestReport.TotalIgnored.ShouldEqual(0);
        }

        [Test]
        [Ignore]
        public void Should_have_a_timeout_exception()
        {
            TestReport
                .TestResults
                .Where(w => w.MethodName == "Async_test_should_timeout")
                .Single()
                .ExceptionInfo.FullMessage.ToLower().ShouldContain("timeout");
        }
    }
}