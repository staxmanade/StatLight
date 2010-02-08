using NUnit.Framework;
using StatLight.Client.Model.Events;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Reporting;
using StatLight.Core.Runners;
using StatLight.Core.Tests;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;
using System;

namespace StatLight.IntegrationTests.ProviderTests.MSTest
{
    [TestFixture]
    public class when_testing_the_runner_with_MSTest_tests
        : IntegrationFixtureBase
    {
        private TestRunConfiguration _testRunConfiguration;
        private TestReport _testReport;
        private InitializationOfUnitTestHarnessClientEvent _initializationOfUnitTestHarnessClientEvent;

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
                                            UnitTestProviderType = UnitTestProviderType.MSTest
                                        };
            base.Before_all_tests();
            IEventAggregator eventAggregator = StatLightRunnerFactory.EventAggregator;


            eventAggregator
                .AddListener<InitializationOfUnitTestHarnessClientEvent>(e => _initializationOfUnitTestHarnessClientEvent = e);

            eventAggregator.SendMessage(new InitializationOfUnitTestHarnessClientEvent());

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

        [Test]
        public void Should_receive_one_InitializationOfUnitTestHarness()
        {
            _initializationOfUnitTestHarnessClientEvent.ShouldNotBeNull();
        }
    }
}