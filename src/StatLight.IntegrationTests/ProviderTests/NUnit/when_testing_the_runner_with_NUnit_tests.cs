using NUnit.Framework;
using StatLight.Core.Reporting;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;

namespace StatLight.IntegrationTests.ProviderTests.NUnit
{
	[TestFixture]
	public class when_testing_the_runner_with_NUnit_tests
		: IntegrationFixtureBase
	{
		private ClientTestRunConfiguration clientTestRunConfiguration;
		private TestReport testReport;

		protected override ClientTestRunConfiguration ClientTestRunConfiguration
		{
			get { return this.clientTestRunConfiguration; }
		}

		protected override void Before_all_tests()
		{
			base.PathToIntegrationTestXap = TestXapFileLocations.NUnit;
			this.clientTestRunConfiguration = new ClientTestRunConfiguration()
			                            	{
			                            		TagFilter = string.Empty,
			                            		UnitTestProviderType = UnitTestProviderType.NUnit
			                            	};
			base.Before_all_tests();

			testReport = base.Runner.Run();
		}


		[Test]
		public void Should_have_correct_TotalFailed_count()
		{
			testReport.TotalFailed.ShouldEqual(1, "Failed count wrong");
		}

		[Test]
		public void Should_have_correct_TotalPassed_count()
		{
			testReport.TotalPassed.ShouldEqual(4, "Passed count wrong");
		}

		[Test]
		public void Should_have_correct_TotalIgnored_count()
		{
			testReport.TotalIgnored.ShouldEqual(1, "Ignored count wrong");
		}
	}
}