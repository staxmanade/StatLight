using System.Linq;
using NUnit.Framework;
using StatLight.Core.Reporting;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using System.Collections.Generic;
using StatLight.Core.WebServer;

namespace StatLight.IntegrationTests.ProviderTests
{
	public abstract class filtered_tests____ : IntegrationFixtureBase
	{
		protected TestReport TestResults { get; set; }
		protected string NormalClassTestName { get; set; }
		protected string NestedClassTestName { get; set; }

		protected override void Before_all_tests()
		{
			NormalClassTestName.ShouldNotBeNull("NormalClassTestName");
			NestedClassTestName.ShouldNotBeNull("NestedClassTestName");

			base.Before_all_tests();
		}

		[Test]
		public void should_execute_a_nested_class_test()
		{
			TestResults
				.Results
				.Where(w => w.TestClassName == NormalClassTestName)
				.Count()
				.ShouldEqual(1);
		}

		[Test]
		public void should_execute_class_test()
		{
			TestResults
				.Results
				.Where(w => w.TestClassName == NestedClassTestName)
				.Count()
				.ShouldEqual(1);
		}

	}
}