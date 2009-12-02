namespace StatLight.Core.Tests
{
	using System;
	using NUnit.Framework;
	using StatLight.Core.UnitTestProviders;
	using StatLight.Core.WebServer;
	using StatLight.IntegrationTests;
	using StatLight.Core.Reporting;

	[TestFixture]
	[Explicit]
	public class when_something_executing_in_silverlight_throws_up_a_modal_MessageBox
		: IntegrationFixtureBase
	{
		private TestRunConfiguration testRunConfiguration;

		protected override TestRunConfiguration TestRunConfiguration
		{
			get { return this.testRunConfiguration; }
		}

		protected override void Before_all_tests()
		{
			this.testRunConfiguration = new TestRunConfiguration()
			{
				TagFilter = "MessageBox",
				UnitTestProviderType = UnitTestProviderType.MSTest
			};
			base.Before_all_tests();
		}

		[Test]
		[Explicit]
		public void the_assertion_monitor_should_click_the_OK_button_and_fail_the_final_result()
		{
			var result = base.Runner.Run();

			result.TotalFailed.ShouldEqual(3);
			result.FinalResult.ShouldEqual(RunCompletedState.Failure);
		}
	}
}
