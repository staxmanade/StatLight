using Moq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Reporting;
using StatLight.Core.Runners;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebBrowser;
using StatLight.Core.WebServer;
using StatLight.Core.Events;
using System.Collections.Generic;

namespace StatLight.Core.Tests.Runners
{
    [TestFixture]
    public class when_ContinuousTestRunner_has_not_gon_through_its_first_test_cycle : FixtureBase
    {
        readonly Mock<IStatLightService> _mockStatLightService = new Mock<IStatLightService>();
        readonly Mock<IWebBrowser> _browserFormHost = new Mock<IWebBrowser>();
        readonly Mock<IXapFileBuildChangedMonitor> _xapFileBuildChangedMonitor = new Mock<IXapFileBuildChangedMonitor>();

        private ContinuousTestRunner CreateContinuousTestRunner()
        {
            var clientTestRunConfiguration = new ClientTestRunConfiguration(UnitTestProviderType.MSTest, new List<string>(), "", 1, "test", WebBrowserType.SelfHosted);
            var runner = new ContinuousTestRunner(TestLogger, TestEventAggregator, _browserFormHost.Object, clientTestRunConfiguration, _xapFileBuildChangedMonitor.Object, "test");
            return runner;
        }

        [Test]
        public void when_creating_the_ContinuousTestRunner_it_should_start_the_test_immediately_and_should_signal_that_a_test_run_is_in_progress()
        {
            var wasStartCalled = false;
            _browserFormHost
                .Setup(x => x.Start())
                .Callback(() => wasStartCalled = true);

            var runner = CreateContinuousTestRunner();

            wasStartCalled.ShouldBeTrue();
            runner.IsCurrentlyRunningTest.ShouldBeTrue();
        }

        [Test]
        public void when_the_test_was_signaled_completed_the_browser_should_have_been_stopped_and_the_ContinuousTestRunner_should_signal_that_it_is_not_currently_running()
        {
            var runner = CreateContinuousTestRunner();

            TestEventAggregator.SendMessage(new TestRunCompletedServerEvent());

            runner.IsCurrentlyRunningTest.ShouldBeFalse();
            _browserFormHost.Verify(x => x.Stop());
        }
    }

    [TestFixture]
    public class when_a_ContinuousTestRunner_has_already_gone_through_the_first_testing_cylce : FixtureBase
    {
        Mock<IWebBrowser> _browserFormHost;
        Mock<IXapFileBuildChangedMonitor> _xapFileBuildChangedMonitor;
        ContinuousTestRunner _continuousTestRunner;
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override void Before_all_tests()
        {
            _browserFormHost = new Mock<IWebBrowser>();
            _xapFileBuildChangedMonitor = new Mock<IXapFileBuildChangedMonitor>();

            base.Before_all_tests();

            _clientTestRunConfiguration = new ClientTestRunConfiguration(UnitTestProviderType.MSTest, new List<string>(), "", 1, "test", WebBrowserType.SelfHosted);
            _continuousTestRunner = new ContinuousTestRunner(TestLogger, TestEventAggregator, _browserFormHost.Object, _clientTestRunConfiguration, _xapFileBuildChangedMonitor.Object, "test");

            // Signal that the first test has already finished.
            TestEventAggregator.SendMessage(new TestRunCompletedServerEvent());
        }

        [Test]
        public void it_should_start_a_new_test_when_the_xap_file_changed()
        {
            _xapFileBuildChangedMonitor.Raise(x => x.FileChanged += null, GetTestXapFileBuildChangedArgs());

            System.Threading.Thread.Sleep(10);

            _continuousTestRunner.IsCurrentlyRunningTest.ShouldBeTrue();
            _browserFormHost.Verify(x => x.Start());
        }

        [Test]
        public void it_should_start_a_new_test_when_the_xap_file_changed_but_not_if_its_already_running_a_test()
        {
            // There's one Start from setup, and one from the first Changed event
            // let's make sure that the second Changed event doesn't fire a start again
            // because we are currently running a test
            _browserFormHost.Setup(s => s.Start()).AtMost(2);

            _xapFileBuildChangedMonitor.Raise(x => x.FileChanged += null, GetTestXapFileBuildChangedArgs());

            // quick test to verify that the test is "running"
            _continuousTestRunner.IsCurrentlyRunningTest.ShouldBeTrue();

            _xapFileBuildChangedMonitor.Raise(x => x.FileChanged += null, GetTestXapFileBuildChangedArgs());
        }

        [Test]
        public void should_be_able_to_force_a_test_run_with_no_filter_and_NOT_have_its_filter_reset_on_forced_test_run_completion()
        {
            const string startTag = "HELLO";
            var newTag = string.Empty;
            _clientTestRunConfiguration.TagFilter = startTag;

            ForceFilteredTestWithTag(newTag);
        }

        [Test]
        public void should_be_able_to_force_a_single_test_run_with_the_specified_filter_and_it_should_not_reset_filter_when_complete()
        {
            const string startTag = "HELLO";
            const string newTag = "TheTempTagFilter";
            _clientTestRunConfiguration.TagFilter = startTag;

            ForceFilteredTestWithTag(newTag);
        }

        private void ForceFilteredTestWithTag(string newTag)
        {
            _continuousTestRunner.ForceFilteredTest(newTag);

            _continuousTestRunner.IsCurrentlyRunningTest.ShouldBeTrue();

            TestEventAggregator.SendMessage(new TestRunCompletedServerEvent());

            _continuousTestRunner.IsCurrentlyRunningTest.ShouldBeFalse();
        }

        private static XapFileBuildChangedEventArgs GetTestXapFileBuildChangedArgs()
        {
            return new XapFileBuildChangedEventArgs();
        }
    }

}
