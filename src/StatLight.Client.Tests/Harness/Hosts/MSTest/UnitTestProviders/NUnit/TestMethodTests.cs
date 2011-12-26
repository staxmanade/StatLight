using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using NUnit.Framework;
using StatLight.Client.Tests;
using StatLight.Core.Events.Hosts.MSTest.UnitTestProviders.NUnit;

namespace StatLight.Core.Events.UnitTestProviders.NUnit
{
    public class MockNUnitTestClass
    {
        [Test]
        public void TestShouldPass()
        { }

        [Test(Description="Description")]
        public void TestShouldBeIgnored()
        { }

        public static MethodInfo GetPassingTest()
        {
            return new MockNUnitTestClass().GetType().GetMethod("TestShouldPass");
        }

        public static MethodInfo GetIgnoreTest()
        {
            return new MockNUnitTestClass().GetType().GetMethod("TestShouldBeIgnored");
        }
    }

    [TestClass]
    public class when_validating_a_standard_nunit_test_attribute_method : FixtureBase
    {
        ITestMethod _testMethod;
        MethodInfo _methodInfo;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            _methodInfo = MockNUnitTestClass.GetPassingTest();
            _testMethod = new TestMethod(_methodInfo);
            _testMethod.ShouldNotBeNull();
        }

        [TestMethod]
        public void Should_work()
        {
            var methodInfo = MockNUnitTestClass.GetPassingTest();
            var testMethod = new TestMethod(methodInfo);
            testMethod.ShouldNotBeNull();

        }

        [TestMethod]
        public void testMethod_should_exist()
        {
            _testMethod.ShouldNotBeNull();
        }

        [TestMethod]
        public void test_method_name_should_be_correct()
        {
            _testMethod.Name.ShouldBeEqualTo(_methodInfo.Name);
        }

        [TestMethod]
        public void the_timeout_value_should_be_correct()
        {
            _testMethod.Timeout.ShouldBeEqualTo(null);
        }

    }
}
