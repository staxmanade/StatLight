using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;

namespace StatLight.IntegrationTests.ProviderTests.XUnit
{
	[TestFixture]
	public class when_testing_the_runner_with_Xunit_tests_filtered_by_certain_methods
		: filtered_tests____
	{
		private ClientTestRunConfiguration _clientTestRunConfiguration;

		protected override ClientTestRunConfiguration ClientTestRunConfiguration
		{
			get { return this._clientTestRunConfiguration; }
		}

		protected override void Before_all_tests()
		{
            base.Before_all_tests();
           
            base.PathToIntegrationTestXap = TestXapFileLocations.XUnit;

            const string namespaceToTestFrom = "StatLight.IntegrationTests.Silverlight.";

            NormalClassTestName = "XunitTests";
            NestedClassTestName = "XunitTests+XunitNestedClassTests";

			_clientTestRunConfiguration = new ClientTestRunConfiguration
			                        	{
			                        		TagFilter = string.Empty,
			                        		UnitTestProviderType = UnitTestProviderType.XUnit,
			                        		MethodsToTest = new List<string>()
			                        		                	{
		                		namespaceToTestFrom + NormalClassTestName + ".this_should_be_a_passing_test",
		                		namespaceToTestFrom + NestedClassTestName + ".this_should_be_a_passing_test",
			                        		                	}
			                        	};


		}
	}
}