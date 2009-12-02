using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Silverlight.Testing;

namespace StatLight.IntegrationTests.Silverlight
{
	[TestClass]
	[Tag("TeamCity")]
	public class TeamCityTests
	{
		[TestMethod]
		public void this_should_be_a_passing_test()
		{
			Assert.IsTrue(true);
		}

		[TestMethod]
		public void this_should_be_a_Failing_test()
		{
			Assert.IsTrue(false);
		}

		[TestMethod]
		public void this_is_an_inconclusive_test()
		{
			Assert.Inconclusive();
		}
	}

	[TestClass]
	[Tag("TeamCity")]
	public class TeamCityTest_with_a_failing_TestInitialize
	{
		[TestInitialize]
		public void setup_should_fail_for_some_reason()
		{
			Assert.Fail("Forcing setup to fail");
		}

		[TestMethod]
		public void this_should_fail_with_message_other_than_HelloWorld()
		{
			Assert.Fail("HelloWorld");
		}
	}
}
