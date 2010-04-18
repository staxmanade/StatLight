using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using StatLight.Core.Reporting;
using StatLight.Core.Tests;

namespace StatLight.IntegrationTests.ProviderTests
{
    public abstract class filtered_tests____ : IntegrationFixtureBase
    {
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
        public virtual void should_execute_nested_class_test()
        {
            TestCaseResults()
                .Where(w => w.ClassName == NestedClassTestName)
                .Count()
                .ShouldEqual(1);
        }

        private IEnumerable<TestCaseResult> TestCaseResults()
        {
            return TestReport.TestResults;
        }
    }
}
