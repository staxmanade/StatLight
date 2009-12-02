namespace StatLight.Core.Tests.WebServer
{
	namespace TestRunConfigurationTests
	{
		using NUnit.Framework;
		using StatLight.Core.UnitTestProviders;
		using StatLight.Core.WebServer;

		[TestFixture]
		public class when_testing_the_default_TestRunConfiguration : FixtureBase
		{
			TestRunConfiguration testRunConfiguration;
			protected override void Before_each_test()
			{
				base.Before_each_test();

				testRunConfiguration = TestRunConfiguration.CreateDefault();
			}

			[Test]
			public void the_default_TagFilter_should_be_an_empty_string()
			{
				testRunConfiguration.TagFilter.ShouldEqual(string.Empty);
			}

			[Test]
			public void the_default_UnitTestProviderType_should_be_MSTest()
			{
				testRunConfiguration.UnitTestProviderType.ShouldEqual(UnitTestProviderType.MSTest);
			}


			[Test]
			public void when_setting_the_TagFilter_to_null_it_should_remain_the_empty_string()
			{
				testRunConfiguration.TagFilter = null;
				testRunConfiguration.TagFilter.ShouldEqual(string.Empty);
			}
		}
	}
}