
namespace StatLight.IntegrationTests.Silverlight
{
	using NUnit.Framework;

	[TestFixture]
	public class NUnitTests
	{
		[TestFixture]
		public class NUnitNestedClassTests
		{
			[Test]
			public void this_should_be_a_passing_test()
			{
				Assert.IsTrue(true);
			}
		}

		private object setupObject;

		[SetUp]
		public void Setup()
		{
			setupObject = new object();
		}

		[Test]
		public void this_should_be_a_passing_test()
		{
			Assert.IsTrue(true);
		}

		[Test]
		public void this_should_also_be_a_passing_test()
		{
			Assert.IsTrue(true);
		}

		[Test]
		public void this_should_be_a_Failing_test()
		{
			Assert.IsTrue(false);
		}

		[Test]
		[Ignore]
		public void this_should_be_an_Ignored_test()
		{
			Assert.Fail("Should have been ignored");
		}

		[Test]
		[Category("xxx")]
		public void the_setup_object_should_not_be_null()
		{
			Assert.IsNotNull(setupObject);
		}


        [Test]
        [TestCase("Hello")]
        public void Should_work_with_TestCase_with_one_parameter(string value)
        {
            Assert.AreEqual("Hello", value);
        }
	}
}
