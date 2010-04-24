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
		private ClientTestRunConfiguration _clientTestRunConfiguration;

		protected override ClientTestRunConfiguration ClientTestRunConfiguration
		{
			get { return this._clientTestRunConfiguration; }
		}

		protected override void Before_all_tests()
		{
            base.Before_all_tests();
           
            base.PathToIntegrationTestXap = TestXapFileLocations.XUnit;
			this._clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(UnitTestProviderType.XUnit);

		}

		[Test]
		public void Should_have_correct_TotalFailed_count()
		{
			TestReport.TotalFailed.ShouldEqual(1);
		}

		[Test]
		public void Should_have_correct_TotalPassed_count()
		{
			TestReport.TotalPassed.ShouldEqual(3);
		}

		[Test]
		public void Should_have_correct_TotalIgnored_count()
		{
			TestReport.TotalIgnored.ShouldEqual(1);
		}
	}
}