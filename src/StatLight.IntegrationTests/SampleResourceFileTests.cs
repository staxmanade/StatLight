using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Reporting;
using StatLight.Core.Tests;

namespace StatLight.IntegrationTests
{
    [TestFixture]
    public class SampleResourceFileTests
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

            _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(
                new List<string>
                   {
                       "StatLight.IntegrationTests.Silverlight.SampleResourceFileTests.Should_be_able_to_read_the_local_xml_file_in_the_xap_under_test",
                   });
        }

        [Test]
        public void the_final_result_should_be_a_failure()
        {
            TestReport.FinalResult.ShouldEqual(RunCompletedState.Successful);
        }

        [Test]
        public void Should_have_not_found_any_failures()
        {
            TestReport.TotalFailed.ShouldEqual(0);
        }

        [Test]
        public void Should_only_have_a_single_result()
        {
            TestReport.TestResults.Count().ShouldEqual(1);
        }
    }
}