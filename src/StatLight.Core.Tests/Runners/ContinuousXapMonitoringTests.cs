﻿using Moq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Runners;
using StatLight.Core.WebBrowser;
using StatLight.Core.Events;
using System.Collections.Generic;

namespace StatLight.Core.Tests.Runners
{
    [TestFixture]
    public class when_ContinuousTestRunner_has_not_gon_through_its_first_test_cycle : FixtureBase
    {
        readonly Mock<IWebBrowser> _mockWebBrowser = new Mock<IWebBrowser>();

        private ContinuousTestRunner CreateContinuousTestRunner()
        {
            var clientTestRunConfiguration = new ClientTestRunConfiguration(UnitTestProviderType.MSTest, new List<string>(), "", 1, WebBrowserType.SelfHosted, false, string.Empty, null);
            var runner = new ContinuousTestRunner(TestLogger, TestEventSubscriptionManager, TestEventPublisher, _mockWebBrowser.Object, clientTestRunConfiguration, "test");
            return runner;
        }

        [Test]
        public void when_creating_the_ContinuousTestRunner_it_should_start_the_test_immediately_and_should_signal_that_a_test_run_is_in_progress()
        {
            var wasStartCalled = false;
            _mockWebBrowser
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

            TestEventPublisher.SendMessage(new TestRunCompletedServerEvent());

            runner.IsCurrentlyRunningTest.ShouldBeFalse();
            _mockWebBrowser.Verify(x => x.Stop());
        }
    }

    [TestFixture]
    public class when_a_ContinuousTestRunner_has_already_gone_through_the_first_testing_cylce : FixtureBase
    {
        Mock<IWebBrowser> _mockWebBrowser;
        ContinuousTestRunner _continuousTestRunner;
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        protected override void Before_all_tests()
        {
            _mockWebBrowser = new Mock<IWebBrowser>();

            base.Before_all_tests();

            _clientTestRunConfiguration = new ClientTestRunConfiguration(UnitTestProviderType.MSTest, new List<string>(), "", 1, WebBrowserType.SelfHosted, false, string.Empty, null);
            _continuousTestRunner = new ContinuousTestRunner(TestLogger, TestEventSubscriptionManager, TestEventPublisher, _mockWebBrowser.Object, _clientTestRunConfiguration, "test");

            // Signal that the first test has already finished.
            TestEventPublisher.SendMessage(new TestRunCompletedServerEvent());
        }

        [Test]
        public void it_should_start_a_new_test_when_the_xap_file_changed()
        {
            TestEventPublisher.SendMessage(new XapFileBuildChangedServerEvent(string.Empty));

            System.Threading.Thread.Sleep(10);

            _continuousTestRunner.IsCurrentlyRunningTest.ShouldBeTrue();
            _mockWebBrowser.Verify(x => x.Start());
        }

        [Test]
        public void it_should_start_a_new_test_when_the_xap_file_changed_but_not_if_its_already_running_a_test()
        {
            // There's one Start from setup, and one from the first Changed event
            // let's make sure that the second Changed event doesn't fire a start again
            // because we are currently running a test
            _mockWebBrowser.Setup(s => s.Start()).AtMost(2);

            TestEventPublisher.SendMessage(new XapFileBuildChangedServerEvent(string.Empty));

            // quick test to verify that the test is "running"
            _continuousTestRunner.IsCurrentlyRunningTest.ShouldBeTrue();

            TestEventPublisher.SendMessage(new XapFileBuildChangedServerEvent(string.Empty));
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

            TestEventPublisher.SendMessage(new TestRunCompletedServerEvent());

            _continuousTestRunner.IsCurrentlyRunningTest.ShouldBeFalse();
        }
    }

}
