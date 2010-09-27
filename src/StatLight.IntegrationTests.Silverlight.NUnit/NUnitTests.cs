
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

        [Test]
        [TestCase("Hello", 1, "World")]
        public void Should_work_with_TestCase_with_three_parameter(string param1, int param2, string param3)
        {
            Assert.AreEqual("Hello", param1);
            Assert.AreEqual(1, param2);
            Assert.AreEqual("World", param3);
        }

        [Test]
        [TestCase("Hello", 1, "World")]
        [TestCase("Hello", 1, "World1")]
        [TestCase("Hello", 1, "World2")]
        [TestCase("Hello", 1, "World3")]
        public void Should_work_with_multiple_TestCases_with_three_parameter(string param1, int param2, string param3)
        {
            Assert.AreEqual("Hello", param1);
            Assert.AreEqual(1, param2);
            Assert.AreEqual("World", param3.Substring(0, 5));
        }

    }
}
