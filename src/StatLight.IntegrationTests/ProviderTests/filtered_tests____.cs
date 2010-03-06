using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StatLight.Core.Reporting;
using StatLight.Core.Tests;

namespace StatLight.IntegrationTests.ProviderTests
{
    public abstract class filtered_tests____ : IntegrationFixtureBase
    {
        protected TestReport TestResults { get; set; }
        protected string NormalClassTestName { get; set; }
        protected string NestedClassTestName { get; set; }

        protected override void Because()
        {
            NormalClassTestName.ShouldNotBeNull("NormalClassTestName");
            NestedClassTestName.ShouldNotBeNull("NestedClassTestName");

            base.Because();
        }

        [Test]
        public void should_execute_a_nested_class_test()
        {
            TestCaseResults()
                .Where(w => w.ClassName == NormalClassTestName)
                .Count()
                .ShouldEqual(1);
        }

        [Test]
        public void should_execute_class_test()
        {
            TestCaseResults()
                .Where(w => w.ClassName == NestedClassTestName)
                .Count()
                .ShouldEqual(1);
        }

        private IEnumerable<TestCaseResult> TestCaseResults()
        {
            return TestResults
                .TestResults
                .Where(w => w is TestCaseResult)
                .Cast<TestCaseResult>();
        }

    }
}