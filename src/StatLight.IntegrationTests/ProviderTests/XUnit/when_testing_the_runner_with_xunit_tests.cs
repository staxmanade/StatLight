using System;
using System.Linq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;

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
        public void Should_get_nested_class_passing_test_result()
        {

            foreach (var r in TestReport.TestResults)
                Console.WriteLine(r.FullMethodName());


            TestReport
                .TestResults
                .Where(w => w.ClassName.SafeEquals("XunitTests+XunitNestedClassTests"))
                .Where(w => w.MethodName.SafeEquals("this_should_be_a_passing_test"))
                .SingleOrDefault()
                .ShouldNotBeNull();
        }

        [Test]
        public void Should_get_passing_test_result()
        {
            TestReport
                .TestResults
                .Where(w => w.ClassName.SafeEquals("XunitTests"))
                .Where(w => w.MethodName.SafeEquals("this_should_be_a_passing_test"))
                .SingleOrDefault()
                .ShouldNotBeNull();
        }

        [Test]
        public void Should_get_another_passing_test_result()
        {
            TestReport
                .TestResults
                .Where(w => w.ClassName.SafeEquals("XunitTests"))
                .Where(w => w.MethodName.SafeEquals("this_should_also_be_a_passing_test"))
                .SingleOrDefault()
                .ShouldNotBeNull();
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

    public static class ExtensionHelper
    {
        public static bool SafeEquals(this string actual, string expected)
        {
            if (actual == null && expected == null)
                return true;
            if (actual == null)
                return false;
            return actual.Equals(expected);
        }
    }
}