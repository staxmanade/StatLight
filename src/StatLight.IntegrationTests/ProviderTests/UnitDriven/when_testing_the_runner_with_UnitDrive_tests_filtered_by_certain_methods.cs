using System;
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
		private TestRunConfiguration _testRunConfiguration;

		protected override TestRunConfiguration TestRunConfiguration
		{
			get { return this._testRunConfiguration; }
		}

		protected override void Before_all_tests()
		{
			base.PathToIntegrationTestXap = TestXapFileLocations.UnitDriven;

			_testRunConfiguration = new TestRunConfiguration
			                        	{
			                        		TagFilter = string.Empty,
			                        		UnitTestProviderType = UnitTestProviderType.UnitDriven,
			                        		MethodsToTest = new List<string>()
			                        		                	{
			                        		                		(base.NormalClassTestName = "StatLight.IntegrationTests.Silverlight.UnitDriven.ExampleTests+UnitDrivenNestedClassTests") + ".this_should_be_a_passing_test",
			                        		                		(base.NestedClassTestName = "StatLight.IntegrationTests.Silverlight.UnitDriven.ExampleTests") + ".EmptyTest",
			                        		                	}
			                        	};

			base.Before_all_tests();

			TestResults = base.Runner.Run();
		}
	}
}