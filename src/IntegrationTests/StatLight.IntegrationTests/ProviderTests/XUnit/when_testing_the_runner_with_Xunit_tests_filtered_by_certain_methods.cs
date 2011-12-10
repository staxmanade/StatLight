namespace StatLight.IntegrationTests.ProviderTests.XUnit
{
    using System.Collections.Generic;
    using global::NUnit.Framework;
    using StatLight.Core.Configuration;

    [TestFixture]
    public class when_testing_the_runner_with_Xunit_tests_filtered_by_certain_methods
        : filtered_tests____
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get
            {
                if (_clientTestRunConfiguration == null)
                {
                    const string namespaceToTestFrom = "StatLight.IntegrationTests.Silverlight.Xunit.";

                    NormalClassTestName = "XunitTests";
                    NestedClassTestName = "XunitTests+XunitNestedClassTests";

                    _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(
                            UnitTestProviderType.Xunit,
                            new List<string>()
	                	    {
                		        namespaceToTestFrom + NormalClassTestName + ".this_should_be_a_passing_test",
                		        namespaceToTestFrom + NestedClassTestName + ".this_should_be_a_passing_test",
	                	    }
                        );
                }

                return this._clientTestRunConfiguration;
            }
        }

        protected override string GetTestXapPath()
        {
            return TestXapFileLocations.XUnit;
        }
    }
}