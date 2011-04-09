using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Tests;

namespace StatLight.IntegrationTests.ProviderTests.NUnit
{
    [TestFixture]
    public class when_testing_the_runner_with_NUnit_tests_filtered_by_an_explicit_test
        : IntegrationFixtureBase
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;
        private string _explicitTestName;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return _clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            PathToIntegrationTestXap = TestXapFileLocations.NUnit;
            const string namespaceAndClass = "StatLight.IntegrationTests.Silverlight.NUnitTests.";

            _explicitTestName = "Should_only_run_Explicitly";

            var fullTestName = namespaceAndClass + _explicitTestName;

            _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(UnitTestProviderType.NUnit,
                                                                                        new List<string>
                                                                                            {
                                                                                                fullTestName,
                                                                                            });
        }

        [Test]
        public void should_have_execute_the_explicit_test()
        {
            TestReport.TestResults
                .Where(w => w.MethodName == _explicitTestName)
                .Count()
                .ShouldEqual(1);
        }

        [Test]
        public void should_execute_a_nested_class_test()
        {
            TestReport.TestResults
                .Count()
                .ShouldEqual(1);
        }
    }
}