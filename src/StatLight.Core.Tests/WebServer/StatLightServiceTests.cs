namespace StatLight.Core.Tests.WebServer
{
    namespace StatLightServiceTests
    {
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using Microsoft.Silverlight.Testing.Harness;
        using NUnit.Framework;
        using StatLight.Core.Common;
        using StatLight.Core.Tests.Mocks;
        using StatLight.Core.UnitTestProviders;
        using StatLight.Core.WebServer;
        using Microsoft.Practices.Composite.Events;
        using StatLight.Core.Events;
        using StatLight.Core.Reporting.Messages;
		using TestOutcome = Core.Reporting.Messages.TestOutcome;
		using LogMessageType = Core.Reporting.Messages.LogMessageType;

        public class with_an_instance_of_the_StatLightService : using_a_random_temp_file_for_testing
        {
            private IStatLightService _statLightService;
            private byte[] _hostXap;

            protected IStatLightService StatLightService
            {
                get { return _statLightService; }
            }

            protected Stream HostXap
            {
                get { return _hostXap.ToStream(); }
            }

            protected override void Before_each_test()
            {
                base.Before_each_test();

                var serverConfig = MockServerTestRunConfiguration;

                _hostXap = serverConfig.HostXap;

                _statLightService = new StatLightService(new NullLogger(), base.TestEventAggregator, base.PathToTempXapFile, TestRunConfiguration.CreateDefault(), serverConfig);
            }
        }

        public class with_the_TestCompleted_event_wired_to_trap_the_TestCompletedArgs : with_an_instance_of_the_StatLightService
        {
            List<MobilScenarioResult> _testMobilScenarioResult;
            List<MobilOtherMessageType> _testMobilOtherMessageType;
            protected List<MobilScenarioResult> TestMobilScenarioResults { get { return _testMobilScenarioResult; } }
            protected List<MobilOtherMessageType> TestMobilOtherMessageType { get { return _testMobilOtherMessageType; } }
            protected bool WasTestCompleteSignalSent { get; private set; }

            protected override void Before_each_test()
            {
                _testMobilScenarioResult = new List<MobilScenarioResult>();
                _testMobilOtherMessageType = new List<MobilOtherMessageType>();

                base.Before_each_test();


                base.TestEventAggregator
                    .GetEvent<TestRunCompletedEvent>()
                    .Subscribe((e) => WasTestCompleteSignalSent = true);

                base.TestEventAggregator
                    .GetEvent<TestResultEvent>()
                    .Subscribe((e) => _testMobilScenarioResult.Add(e));

                base.TestEventAggregator
                    .GetEvent<TestHarnessOtherMessageEvent>()
                    .Subscribe((e) => _testMobilOtherMessageType.Add(e));

                var postCount = PostMessagesToService();

                StatLightService.SignalTestComplete(postCount);
            }

            protected virtual int PostMessagesToService()
            {
                return 0;
            }
        }

        [TestFixture]
        public class when_testing_messages_posted_to_the_service : with_the_TestCompleted_event_wired_to_trap_the_TestCompletedArgs
        {
            protected override void Before_each_test()
            {
                base.Before_each_test();
            }
            protected override int PostMessagesToService()
            {
                StatLightService.PostMessage(MessageFactory.CreateOtherMessageTypeStream(LogMessageType.Error));
                StatLightService.PostMessage(MessageFactory.CreateResultStream(TestOutcome.Passed));
                StatLightService.PostMessage(MessageFactory.CreateResultStream(TestOutcome.Failed));

                return 3;
            }

            [Test]
            public void the_service_should_have_thrown_a_faild_test_event()
            {
                TestMobilScenarioResults.Where(w => w.Result == TestOutcome.Failed)
                    .Count().ShouldEqual(1);
            }

            [Test]
            public void the_service_should_have_thrown_a_passing_test_event()
            {
                TestMobilScenarioResults.Where(w => w.Result == TestOutcome.Passed)
                    .Count().ShouldEqual(1);
            }

            [Test]
            public void the_service_should_have_thrown_an_other_message_that_is_an_error()
            {
                TestMobilOtherMessageType.Where(w => w.MessageType == LogMessageType.Error)
                    .Count().ShouldEqual(1);
            }
        }
        [TestFixture]
        public class when_using_an_instance_of_the_StatLightService_that_is_initialized_with_filtering_options : using_a_random_temp_file_for_testing
        {
            private const string _tagFilter = "TEST";
            IStatLightService _statLightService;

            protected override void Before_each_test()
            {
                base.Before_each_test();

                var config = new TestRunConfiguration();
                config.TagFilter = _tagFilter;

                _statLightService = new StatLightService(new NullLogger(), base.TestEventAggregator, base.PathToTempXapFile, config, MockServerTestRunConfiguration);
            }

            [Test]
            public void should_be_able_to_get_the_startup_options()
            {
                _statLightService.GetTestRunConfiguration().ShouldNotBeNull();
            }


            [Test]
            public void the_default_UnitTestProviderType_should_be_Undefined()
            {
                _statLightService.GetTestRunConfiguration().UnitTestProviderType
                    .ShouldEqual(UnitTestProviderType.Undefined);
            }

			//[Test]
			//public void should_be_able_to_override_the_default_UnitTestProviderType()
			//{
			//    _statLightService.GetTestRunConfiguration().UnitTestProviderType = StatLight.Core.UnitTestProviders.UnitTestProviderType.XUnit;
			//    _statLightService.GetTestRunConfiguration().UnitTestProviderType
			//        .ShouldEqual(UnitTestProviderType.XUnit);
			//}

        }

        [TestFixture]
        public class when_testing_the_use_of_an_instance_of_the_StatLightService_it : with_an_instance_of_the_StatLightService
        {

            [Test]
            public void should_throw_FileNotFoundException_when_given_bad_file_path()
            {
                typeof(FileNotFoundException).ShouldBeThrownBy(() => new StatLightService(new NullLogger(), base.TestEventAggregator, "missingFile", TestRunConfiguration.CreateDefault(), MockServerTestRunConfiguration));
            }

            [Test]
            public void should_be_able_to_get_the_same_hostXap_as_configured_in_the_config()
            {
                var serviceXap = StatLightService.GetTestPageHostXap().ReadByte();
                var configuredXap = HostXap.ReadByte();

                serviceXap.ShouldEqual(configuredXap);
            }

            [Test]
            public void should_be_able_to_request_the_ClientAccessPolicy()
            {
                var crossDomainPolicy = StatLightService.GetCrossDomainPolicy();

                crossDomainPolicy.ShouldNotBeNull();
                crossDomainPolicy.ShouldContain("cross-domain-policy");
            }

            [Test]
            public void should_be_able_to_request_the_host_html_TestPage()
            {
                var crossDomainPolicy = StatLightService.GetHtmlTestPage();

                crossDomainPolicy.ShouldNotBeNull();
            }

            [Test]
            public void when_the_test_has_signaled_completion_subscriber_to_TestComplete_should_be_notified()
            {
                bool wasSignaledTestComplete = false;

                TestEventAggregator.GetEvent<TestRunCompletedEvent>().Subscribe(o => wasSignaledTestComplete = true);

                StatLightService.SignalTestComplete(0);

                wasSignaledTestComplete.ShouldBeTrue();
            }

            [Test]
            public void it_should_wait_for_all_post_messages_to_arrive_if_the_TestComplete_signal_comes_in_with_a_message_count_greater_than_the_currently_posted()
            {
                bool wasSignaledTestComplete = false;

                TestEventAggregator.GetEvent<TestRunCompletedEvent>().Subscribe(o => wasSignaledTestComplete = true);

                // Signal completion of the test with a total of 2 messages 
                // (that should have been posted to the server)
                StatLightService.SignalTestComplete(2);
                wasSignaledTestComplete.ShouldBeFalse();

                // Post the first 1
                StatLightService.PostMessage(MessageFactory.CreateOtherMessageTypeStream(LogMessageType.Debug));
                wasSignaledTestComplete.ShouldBeFalse();

                // Post the second one and the event should have been fired.
                StatLightService.PostMessage(MessageFactory.CreateOtherMessageTypeStream(LogMessageType.Debug));
                wasSignaledTestComplete.ShouldBeTrue();

            }

        }

        [TestFixture]
        public class when_completing_two_separate_test_runs : with_the_TestCompleted_event_wired_to_trap_the_TestCompletedArgs
        {
            [Test]
            public void when_one_message_was_posted_for_two_seperate_test_runs()
            {
                bool wasSignaledTestComplete = false;
                TestEventAggregator.GetEvent<TestRunCompletedEvent>().Subscribe(o => wasSignaledTestComplete = true);

                StatLightService.PostMessage(MessageFactory.CreateResultStream(TestOutcome.Passed));
                StatLightService.SignalTestComplete(1);
                System.Threading.Thread.Sleep(10);

                wasSignaledTestComplete.ShouldBeTrue();
                //TestCompletedArgs.ShouldNotBeNull();
                //TestCompletedArgs.TotalClientMessageSentCount.ShouldEqual(1);

                wasSignaledTestComplete = false;

                StatLightService.PostMessage(MessageFactory.CreateResultStream(TestOutcome.Passed));
                StatLightService.SignalTestComplete(1);
                System.Threading.Thread.Sleep(10);

                wasSignaledTestComplete.ShouldBeTrue();
                //TestCompletedArgs.ShouldNotBeNull();
                //TestCompletedArgs.TotalClientMessageSentCount.ShouldEqual(1);
            }
        }

        [TestFixture]
        public class when_testing_the_service_TestCompletedArgs : with_the_TestCompleted_event_wired_to_trap_the_TestCompletedArgs
        {
            private int _messagesSent;

            protected override void Before_each_test()
            {
                _messagesSent = 0;
                base.Before_each_test();
            }

            List<TestOutcome> outcomes = new List<TestOutcome>()
			{
				TestOutcome.Passed,
				TestOutcome.Failed,
				TestOutcome.Passed,
				TestOutcome.Passed,
				TestOutcome.Passed,
				TestOutcome.Passed,
				TestOutcome.Failed,
				TestOutcome.Failed,
			};

            List<LogMessageType> otherMessageTypes = new List<LogMessageType>()
			{
				LogMessageType.Error,
				LogMessageType.Error,
				LogMessageType.Error,
			};

            protected void PostMessage(Stream message)
            {
                StatLightService.PostMessage(message);
                _messagesSent++;
            }

            protected override int PostMessagesToService()
            {

                foreach (var message in otherMessageTypes)
                    PostMessage(MessageFactory.CreateOtherMessageTypeStream(message));

                foreach (var outcome in outcomes)
                    PostMessage(MessageFactory.CreateResultStream(outcome));

                return _messagesSent;
            }

            [Test]
            public void should_have_received_the_TestCompletedArgs()
            {
                base.WasTestCompleteSignalSent.ShouldBeTrue();
            }
        }
    }
}