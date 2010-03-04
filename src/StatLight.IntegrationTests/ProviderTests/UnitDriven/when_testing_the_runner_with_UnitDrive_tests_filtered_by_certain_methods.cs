using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;

namespace StatLight.IntegrationTests.ProviderTests.UnitDriven
{
	[TestFixture]
	public class when_testing_the_runner_with_UnitDrive_tests_filtered_by_certain_methods
		: filtered_tests____
	{
		private ClientTestRunConfiguration _clientTestRunConfiguration;

		protected override ClientTestRunConfiguration ClientTestRunConfiguration
		{
			get { return _clientTestRunConfiguration; }
		}

		protected override void Before_all_tests()
		{
			PathToIntegrationTestXap = TestXapFileLocations.UnitDriven;

            const string namespaceToTestFrom = "StatLight.IntegrationTests.Silverlight.UnitDriven.";

            NormalClassTestName = "ExampleTests";
            NestedClassTestName = "ExampleTests+UnitDrivenNestedClassTests";

			_clientTestRunConfiguration = new ClientTestRunConfiguration
			                        	{
			                        		TagFilter = string.Empty,
			                        		UnitTestProviderType = UnitTestProviderType.UnitDriven,
			                        		MethodsToTest = new List<string>
			                        		                    {
		                		namespaceToTestFrom + NormalClassTestName + ".EmptyTest",
		                		namespaceToTestFrom + NestedClassTestName + ".this_should_be_a_passing_test",
			                        		                	}
			                        	};

			base.Before_all_tests();

			TestResults = base.Runner.Run();
		}
	}
}