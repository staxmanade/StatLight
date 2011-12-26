using System;
using System.Linq;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StatLight.Client.Tests;
using StatLight.Core.Events.Hosts.MSTest.UnitTestProviders.NUnit;

namespace StatLight.Core.Events.UnitTestProviders.NUnit
{
    [TestClass]
    public class NUnitTestProviderTests : FixtureBase
    {
        IUnitTestProvider provider;
        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            provider = new NUnitTestProvider();
        }

        [TestMethod]
        public void provider_should_support_MethodCanIgnore()
        {
            provider
                .HasCapability(UnitTestProviderCapabilities.MethodCanIgnore)
                .ShouldBeTrue();
        }
    }

    [TestClass]
    public class NUnitTestMethodTests
    {
        private TestClass testClass;
        public NUnitTestMethodTests()
        {
            testClass = new TestClass((IAssembly)null, typeof(NUnitSampleTestClass));
        }


        [TestMethod]
        public void Should_find_correct_test_fixture_setup()
        {
            testClass.ClassInitializeMethod.Name.ShouldBeEqualTo("FixtureSetUp");
        }

        [TestMethod]
        public void Should_find_correct_test_fixture_teardown()
        {
            testClass.ClassCleanupMethod.Name.ShouldBeEqualTo("FixtureTearDown");
        }

        [TestMethod]
        public void Should_find_correct_test_setup_method()
        {
            testClass.TestInitializeMethod.Name.ShouldBeEqualTo("SetUp");
        }

        [TestMethod]
        public void Should_find_correct_test_teardown()
        {
            testClass.TestCleanupMethod.Name.ShouldBeEqualTo("TearDown");
        }

        [TestMethod]
        public void Should_create_distinct_method_names_for_TestCase_tests()
        {
            ITestMethod[] testMethods = GetTestMethodsForTest("TestCaseTest");
            testMethods.Length.ShouldBeEqualTo(2);

            testMethods[0].Name.ShouldBeEqualTo("TestCaseTest(\"a\", 1, \"b\")");
            testMethods[1].Name.ShouldBeEqualTo("TestCaseTest(\"a\", 1, \"c\")");
        }

        [TestMethod]
        public void Should_report_correct_TestName_for_specified_TestCase_test_names()
        {
            ITestMethod testMethods = GetTestMethodsForTest("TestCaseWithName").FirstOrDefault();
            testMethods.ShouldNotBeNull();

            // funny the attributes come back out of order???
            testMethods.Name.ShouldBeEqualTo("SomeManuallyControlledTestName");
        }
        

        private ITestMethod[] GetTestMethodsForTest(string testName)
        {
            return testClass.GetTestMethods()
                .Where(w => w.Method.Name == testName)
                .OrderBy(ob => ob.Name)
                .ToArray();
        }

    }


    [TestFixture]
    public class NUnitSampleTestClass
    {
        [TestFixtureSetUp]
        public void FixtureSetUp()
        {
            Console.WriteLine("Test Fixture SetUp...");
        }

        [TestFixtureTearDown]
        public void FixtureTearDown()
        {
            Console.WriteLine("Test Fixture TearDown...");
        }

        [SetUp]
        public void SetUp()
        {
            Console.WriteLine("Setup...");
        }

        [TearDown]
        public void TearDown()
        {
            Console.WriteLine("TearDown...");
        }

        [Test]
        public void Test1()
        {
            Console.WriteLine("Test1...");
            Assert.AreEqual(1, 1);
        }

        [Test]
        [TestCase("a", 1, "b")]
        [TestCase("a", 1, "c")]
        public void TestCaseTest(string p1, int p2, string p3)
        {
            Assert.AreEqual("a", p1);
            Assert.AreEqual(1, p2);

            if (!(p3 == "b" || p3 == "c"))
            {
                Assert.Fail("p3=[" + p3 + "] should either be 'b' or 'c' ");
            }
        }

        [Test]
        [TestCase("a", TestName = "SomeManuallyControlledTestName")]
        public void TestCaseWithName(string p1)
        {
            Assert.AreEqual("a", p1);
        }
    }
}


