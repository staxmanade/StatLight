using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Core.Configuration;

namespace StatLight.IntegrationTests.ProviderTests.MSTest
{
    [TestFixture]
    public class when_testing_the_runner_with_MSTest_tests_filtered_by_certain_methods
        : filtered_tests____
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get
            {
                if (_clientTestRunConfiguration == null)
                {
                    const string namespaceToTestFrom = "StatLight.IntegrationTests.Silverlight.";

                    NormalClassTestName = "MSTestTests";
                    NestedClassTestName = "MSTestTests+MSTestNestedClassTests";

                    _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(
                        new List<string>
                        {
                            namespaceToTestFrom + NormalClassTestName + ".this_should_be_a_passing_test",
                            namespaceToTestFrom + NestedClassTestName + ".this_should_be_a_passing_test",
                        }
                    );
                }

                return _clientTestRunConfiguration;
            }
        }


        protected override string GetTestXapPath()
        {
            return TestXapFileLocations.MSTestSL4;
        }
    }

}