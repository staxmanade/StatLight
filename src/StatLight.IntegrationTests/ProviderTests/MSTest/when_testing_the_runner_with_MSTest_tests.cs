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
            _testExecutionClassBeginClientEvent.Count().ShouldEqual(1);
            AssertTestExecutionClassData(_testExecutionClassBeginClientEvent.Single());
        }

        [Test]
        public void Should_receive_the_TestExecutionClassCompletedClientEvent()
        {
            _testExecutionClassCompletedClientEvent.Count().ShouldEqual(1);
            AssertTestExecutionClassData(_testExecutionClassCompletedClientEvent.Single());
        }

        //[Test]
        //public void Should_receive_the_TestExecutionMethodBeginClientEvent()
        //{
        //    _testExecutionMethodBeginClientEvent.Count().ShouldEqual(5);
        //}

        private static void AssertTestExecutionClassData(TestExecutionClass e)
        {
            e.NamespaceName.ShouldEqual("StatLight.IntegrationTests.Silverlight", "{0} - NamespaceName property should be correct.".FormatWith(e.GetType().FullName));
            e.ClassName.ShouldEqual("MSTest", "{0} - ClassName property should be correct.".FormatWith(e.GetType().FullName));
        }
        #endregion
    }
}