using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Core.Configuration;

namespace StatLight.IntegrationTests.ProviderTests.UnitDriven
{
    [TestFixture]
    public class when_testing_the_runner_with_UnitDriven_tests_filtered_by_certain_methods
        : filtered_tests____
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get
            {
                if (_clientTestRunConfiguration == null)
                {

                    const string namespaceToTestFrom = "StatLight.IntegrationTests.Silverlight.UnitDriven.";

                    NormalClassTestName = "ExampleTests";
                    NestedClassTestName = "ExampleTests+UnitDrivenNestedClassTests";

                    _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration (
                            UnitTestProviderType.UnitDriven,
                            new List<string>
                            {
                                namespaceToTestFrom + NormalClassTestName + ".EmptyTest",
                                namespaceToTestFrom + NestedClassTestName + ".this_should_be_a_passing_test",
                            }
                        );
                }

                return _clientTestRunConfiguration;
            }
        }

        protected override string GetTestXapPath()
        {
            return TestXapFileLocations.UnitDriven;
        }

        [Ignore("UnitDriven's host doesn't do Nested classes.")]
        public override void should_execute_nested_class_test()
        {
        }
    }
}