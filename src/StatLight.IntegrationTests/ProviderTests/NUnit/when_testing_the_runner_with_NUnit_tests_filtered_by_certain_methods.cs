using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Core.Configuration;

namespace StatLight.IntegrationTests.ProviderTests.NUnit
{
    [TestFixture]
    public class when_testing_the_runner_with_NUnit_tests_filtered_by_certain_methods
        : filtered_tests____
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return _clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();
            
            PathToIntegrationTestXap = TestXapFileLocations.NUnit;
            const string namespaceToTestFrom = "StatLight.IntegrationTests.Silverlight.";

            NormalClassTestName = "NUnitTests";
            NestedClassTestName = "NUnitTests+NUnitNestedClassTests";

            _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(UnitTestProviderType.NUnit,
                new List<string>
                        {
                            namespaceToTestFrom + NormalClassTestName + ".this_should_be_a_passing_test",
                            namespaceToTestFrom + NestedClassTestName + ".this_should_be_a_passing_test",
                        });
        }
    }
}