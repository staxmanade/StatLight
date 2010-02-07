using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StatLight.Core.Reporting;
using StatLight.Core.Reporting.Messages;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;

namespace StatLight.IntegrationTests.ProviderTests.UnitDriven
{
    [TestFixture]
    public class when_testing_the_runner_with_UnitDriven_tests
        : IntegrationFixtureBase
    {
        private TestRunConfiguration _testRunConfiguration;
        private TestReport _testReport;

        protected override TestRunConfiguration TestRunConfiguration
        {
            get { return this._testRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.PathToIntegrationTestXap = TestXapFileLocations.UnitDriven;
            this._testRunConfiguration = new TestRunConfiguration()
                                            {
                                                TagFilter = string.Empty,
                                                UnitTestProviderType = UnitTestProviderType.UnitDriven
                                            };
            base.Before_all_tests();

            _testReport = base.Runner.Run();
        }

        [Test]
        public void Should_have_correct_TotalFailed_count()
        {
            _testReport.TotalFailed.ShouldEqual(4);
        }

        [Test]
        [Ignore]
        public void Should_have_error_message_for_async_failed_tests()
        {
            _testReport
                .Results
                .Where(w => w.Result == TestOutcome.Failed)
                .Where(w => string.IsNullOrEmpty(w.ExceptionMessage))
                .Count().ShouldBeLessThan(1);
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