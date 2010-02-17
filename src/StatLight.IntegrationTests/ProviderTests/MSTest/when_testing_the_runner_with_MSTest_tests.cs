using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Client.Model.Events;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Reporting;
using StatLight.Core.Runners;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;

namespace StatLight.IntegrationTests.ProviderTests.MSTest
{
    [TestFixture]
    public class when_testing_the_runner_with_MSTest_tests
        : IntegrationFixtureBase
    {
        private TestRunConfiguration _testRunConfiguration;
        private TestReport _testReport;
        private InitializationOfUnitTestHarnessClientEvent _initializationOfUnitTestHarnessClientEvent;

        private readonly IList<TestExecutionClassBeginClientEvent> _testExecutionClassBeginClientEvent =
            new List<TestExecutionClassBeginClientEvent>();

        private readonly IList<TestExecutionClassCompletedClientEvent> _testExecutionClassCompletedClientEvent =
            new List<TestExecutionClassCompletedClientEvent>();
        private readonly IList<TestExecutionMethodBeginClientEvent> _testExecutionMethodBeginClientEvent =
            new List<TestExecutionMethodBeginClientEvent>();
        private readonly IList<TestExecutionMethodIgnoredClientEvent> _testExecutionMethodIgnoredClientEvent =
            new List<TestExecutionMethodIgnoredClientEvent>();
        private readonly IList<TestExecutionMethodFailedClientEvent> _testExecutionMethodFailedClientEvent =
            new List<TestExecutionMethodFailedClientEvent>();

        protected override TestRunConfiguration TestRunConfiguration
        {
            get { return _testRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            PathToIntegrationTestXap = TestXapFileLocations.MSTest;
            _testRunConfiguration = new TestRunConfiguration
                                        {
                                            TagFilter = string.Empty,
                                            UnitTestProviderType = UnitTestProviderType.MSTest,
                                        };
            base.Before_all_tests();
            IEventAggregator eventAggregator = StatLightRunnerFactory.EventAggregator;


            eventAggregator
                .AddListener<InitializationOfUnitTestHarnessClientEvent>(e => _initializationOfUnitTestHarnessClientEvent = e)
                .AddListener<TestExecutionClassBeginClientEvent>(e => _testExecutionClassBeginClientEvent.Add(e))
                .AddListener<TestExecutionClassCompletedClientEvent>(e => _testExecutionClassCompletedClientEvent.Add(e))
                .AddListener<TestExecutionMethodBeginClientEvent>(e => _testExecutionMethodBeginClientEvent.Add(e))
                .AddListener<TestExecutionMethodIgnoredClientEvent>(e => _testExecutionMethodIgnoredClientEvent.Add(e))
                .AddListener<TestExecutionMethodFailedClientEvent>(e => _testExecutionMethodFailedClientEvent.Add(e))
                ;

            _testReport = Runner.Run();
        }


        [Test]
        public void Should_have_correct_TotalFailed_count()
        {
            _testReport.TotalFailed.ShouldEqual(1);
        }

        [Test]
        public void Should_have_correct_TotalPassed_count()
        {
            _testReport.TotalPassed.ShouldEqual(4);
        }

        [Test]
        public void Should_have_correct_TotalIgnored_count()
        {
            _testReport.TotalIgnored.ShouldEqual(1);
        }

        #region Events

        [Test]
        public void Should_receive_one_InitializationOfUnitTestHarness()
        {
            _initializationOfUnitTestHarnessClientEvent.ShouldNotBeNull();
        }

        [Test]
        public void Should_receive_the_TestExecutionClassBeginClientEvent()
        {
            _testExecutionClassBeginClientEvent.Count().ShouldEqual(2);
            _testExecutionClassBeginClientEvent.Each(AssertTestExecutionClassData);
        }

        [Test]
        public void Should_receive_the_TestExecutionClassCompletedClientEvent()
        {
            _testExecutionClassCompletedClientEvent.Count().ShouldEqual(2);
            _testExecutionClassCompletedClientEvent.Each(AssertTestExecutionClassData);
        }

        [Test]
        public void Should_receive_the_TestExecutionMethodBeginClientEvent()
        {
            _testExecutionMethodBeginClientEvent.Count().ShouldEqual(5);
            foreach (var e in _testExecutionMethodBeginClientEvent)
                AssertTestExecutionClassData(e);
        }

        [Test]
        public void Should_receive_the_TestExecutionMethodIgnoredClientEvent()
        {
            _testExecutionMethodIgnoredClientEvent.Count().ShouldEqual(1);
            _testExecutionMethodIgnoredClientEvent.First().MethodName.ShouldEqual("this_should_be_an_Ignored_test");
        }

        [Test]
        public void Should_receive_the_TestExecutionMethodFailedClientEvent()
        {
            _testExecutionMethodFailedClientEvent.Count().ShouldEqual(1);
        }

        private static void AssertTestExecutionClassData(TestExecutionClass e)
        {
            e.NamespaceName.ShouldEqual("StatLight.IntegrationTests.Silverlight", "{0} - NamespaceName property should be correct.".FormatWith(e.GetType().FullName));

            var validClassNames = new List<string> { "MSTestNestedClassTests", "MSTestTests" };
            if (!validClassNames.Contains(e.ClassName))
                Assert.Fail("e.ClassName is not equal to MSTestNestedClassTests or MSTestTest - actual=" + e.ClassName);
        }
        #endregion
    }
}