using System;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit;
using StatLight.Client.Tests;

namespace StatLight.Client.Harness.UnitTestProviders.NUnit
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
    }
}


