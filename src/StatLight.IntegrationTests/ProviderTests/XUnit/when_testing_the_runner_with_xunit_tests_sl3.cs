namespace StatLight.IntegrationTests.ProviderTests.XUnit
{
    using System;
    using System.Linq;
    using global::NUnit.Framework;
    using StatLight.Core.Configuration;
    using StatLight.Core.Tests;

    [TestFixture]
    public class when_testing_the_runner_with_xunit_tests_sl3
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

            base.PathToIntegrationTestXap = TestXapFileLocations.XUnitSL3;
            this._clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration(UnitTestProviderType.Xunit);
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
            TestReport.TotalPassed.ShouldEqual(4);
        }

        [Test]
        public void Should_have_correct_TotalIgnored_count()
        {
            TestReport.TotalIgnored.ShouldEqual(1);
        }
    }
}