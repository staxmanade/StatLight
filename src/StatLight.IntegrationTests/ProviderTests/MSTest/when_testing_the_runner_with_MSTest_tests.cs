using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Client.Harness.Events;
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
        private ClientTestRunConfiguration _clientTestRunConfiguration;
        private InitializationOfUnitTestHarnessClientEvent _initializationOfUnitTestHarnessClientEvent;

        private readonly IList<TestExecutionClassBeginClientEvent> _testExecutionClassBeginClientEvent = new List<TestExecutionClassBeginClientEvent>();
        private readonly IList<TestExecutionClassCompletedClientEvent> _testExecutionClassCompletedClientEvent = new List<TestExecutionClassCompletedClientEvent>();
        private readonly IList<TestExecutionMethodBeginClientEvent> _testExecutionMethodBeginClientEvent = new List<TestExecutionMethodBeginClientEvent>();
        private readonly IList<TestExecutionMethodIgnoredClientEvent> _testExecutionMethodIgnoredClientEvent = new List<TestExecutionMethodIgnoredClientEvent>();
        private readonly IList<TestExecutionMethodFailedClientEvent> _testExecutionMethodFailedClientEvent = new List<TestExecutionMethodFailedClientEvent>();
        private readonly IList<TestExecutionMethodPassedClientEvent> _testExecutionMethodPassedClientEvent = new List<TestExecutionMethodPassedClientEvent>();

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return _clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            PathToIntegrationTestXap = TestXapFileLocations.MSTest;
            _clientTestRunConfiguration = new ClientTestRunConfiguration
                                        {
                                            TagFilter = string.Empty,
                                            UnitTestProviderType = UnitTestProviderType.MSTest,
                                        };
            EventAggregator
                .AddListener<InitializationOfUnitTestHarnessClientEvent>(e => _initializationOfUnitTestHarnessClientEvent = e)
                .AddListener<TestExecutionClassBeginClientEvent>(e => _testExecutionClassBeginClientEvent.Add(e))
                .AddListener<TestExecutionClassCompletedClientEvent>(e => _testExecutionClassCompletedClientEvent.Add(e))
                .AddListener<TestExecutionMethodBeginClientEvent>(e => _testExecutionMethodBeginClientEvent.Add(e))
                .AddListener<TestExecutionMethodIgnoredClientEvent>(e => _testExecutionMethodIgnoredClientEvent.Add(e))
                .AddListener<TestExecutionMethodFailedClientEvent>(e => _testExecutionMethodFailedClientEvent.Add(e))
                .AddListener<TestExecutionMethodPassedClientEvent>(e => _testExecutionMethodPassedClientEvent.Add(e))
                ;

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
            //AssertTestExecutionClassData(_testExecutionMethodIgnoredClientEvent.First());
            //TODO: figure out how to get the class/namespace for the ignored test.
        }

        [Test]
        public void Should_receive_the_TestExecutionMethodFailedClientEvent()
        {
            _testExecutionMethodFailedClientEvent.Count().ShouldEqual(1);

            var e = _testExecutionMethodFailedClientEvent.First();

            AssertTestExecutionClassData(e);
            //TODO: assert other properties of the failed exception?

            e.Finished.ShouldNotEqual(new DateTime());
            e.Started.ShouldNotEqual(new DateTime());
        }

        [Test]
        public void Should_receive_the_TestExecutionMethodPassedClientEvent()
        {
            _testExecutionMethodPassedClientEvent.Count.ShouldEqual(4);
        }

        private static void AssertTestExecutionClassData(TestExecutionClass e)
        {
            e.NamespaceName.ShouldEqual("StatLight.IntegrationTests.Silverlight", "{0} - NamespaceName property should be correct.".FormatWith(e.GetType().FullName));

            var validClassNames = new List<string> { "MSTestTests+MSTestNestedClassTests", "MSTestTests" };
            if (!validClassNames.Contains(e.ClassName))
                Assert.Fail("e.ClassName is not equal to MSTestNestedClassTests or MSTestTest - actual=" + e.ClassName);
        }
        #endregion
    }
}