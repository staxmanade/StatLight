using NUnit.Framework;
using StatLight.Core.Reporting;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;
using StatLight.Core.WebServer.XapInspection;

namespace StatLight.IntegrationTests.ProviderTests.XUnit
{
	[TestFixture]
	public class when_testing_the_runner_with_xunit_tests
		: IntegrationFixtureBase
	{
		private TestRunConfiguration _testRunConfiguration;
		private TestReport _testReport;

		protected override TestRunConfiguration TestRunConfiguration
		{
			get { return this._testRunConfiguration; }
		}

		protected override void Before_all_tests()
		{
			base.PathToIntegrationTestXap = TestXapFileLocations.XUnit;
			this._testRunConfiguration = new TestRunConfiguration
			                             	{
			                             		TagFilter = string.Empty,
			                             		UnitTestProviderType = UnitTestProviderType.XUnit
			                             	};
			base.Before_all_tests();

			_testReport = base.Runner.Run();
		}

		[Test]
		public void Should_have_correct_TotalFailed_count()
		{
			_testReport.TotalFailed.ShouldEqual(1);
		}

		[Test]
		public void Should_have_correct_TotalPassed_count()
		{
			_testReport.TotalPassed.ShouldEqual(3);
		}

		[Test]
		public void Should_have_correct_TotalIgnored_count()
		{
			_testReport.TotalIgnored.ShouldEqual(1);
		}
	}
}