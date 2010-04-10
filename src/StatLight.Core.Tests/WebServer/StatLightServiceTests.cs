
using System;
using StatLight.Core.Reporting;

namespace StatLight.Core.Tests.WebServer
{
    namespace StatLightServiceTests
    {
        using System.Collections.Generic;
        using System.IO;
        using System.Linq;
        using NUnit.Framework;
        using StatLight.Client.Harness.Events;
        using StatLight.Core.Common;
        using StatLight.Core.Events;
        using StatLight.Core.Reporting.Messages;
        using StatLight.Core.Serialization;
        using StatLight.Core.Tests.Mocks;
        using StatLight.Core.UnitTestProviders;
        using StatLight.Core.WebServer;
        using LogMessageType = Core.Reporting.Messages.LogMessageType;
        using TestOutcome = Core.Reporting.Messages.TestOutcome;

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

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                var serverConfig = MockServerTestRunConfiguration;

                _hostXap = serverConfig.HostXap;

                _statLightService = new StatLightService(new NullLogger(), TestEventAggregator, PathToTempXapFile, ClientTestRunConfiguration.CreateDefault(), serverConfig);
            }

            protected void SignalTestComplete(IStatLightService statLightService, int postCount)
            {
                var signalCompleteMsg = (new SignalTestCompleteClientEvent
                {
                    TotalMessagesPostedCount = postCount
                })
                          .Serialize()
                          .ToStream();

                statLightService.PostMessage(signalCompleteMsg);
            }
        }

        public class with_the_TestCompleted_event_wired_to_trap_the_TestCompletedArgs
            : with_an_instance_of_the_StatLightService
        {
            protected readonly List<UnhandledExceptionClientEvent> _errors = new List<UnhandledExceptionClientEvent>();
            protected bool WasTestCompleteSignalSent { get; private set; }
            protected TestResultAggregator TestResultAggregator { get; private set; }
            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                TestResultAggregator = new TestResultAggregator(TestLogger, TestEventAggregator);
                TestEventAggregator.AddListener(TestResultAggregator);

                TestEventAggregator
                    .AddListener<TestRunCompletedServerEvent>(e => WasTestCompleteSignalSent = true);


                TestEventAggregator
                    .AddListener<UnhandledExceptionClientEvent>(e => _errors.Add(e));

                var postCount = PostMessagesToService();

                SignalTestComplete(StatLightService, postCount);
            }

            protected virtual int PostMessagesToService()
            {
                return 0;
            }
        }

        [TestFixture]
        public class when_testing_messages_posted_to_the_service : with_the_TestCompleted_event_wired_to_trap_the_TestCompletedArgs
        {
            protected override int PostMessagesToService()
            {
                var events = new ClientEvent[]
                            {
                                new UnhandledExceptionClientEvent(),
                                new TestExecutionMethodBeginClientEvent{MethodName = "a"},
                                new TestExecutionMethodPassedClientEvent{MethodName = "a"},
                                new TestExecutionMethodBeginClientEvent{MethodName = "b"},
                                new TestExecutionMethodFailedClientEvent{MethodName = "b"},
                            };
                foreach (var e in events)
                    StatLightService.PostMessage(e.Serialize().ToStream());

                return 3;
            }

            [Test]
            public void the_service_should_have_thrown_a_faild_test_event()
            {
                TestResultAggregator.CurrentReport.TotalFailed.ShouldEqual(1);
            }

            [Test]
            public void the_service_should_have_thrown_a_passing_test_event()
            {
                TestResultAggregator.CurrentReport.TotalPassed.ShouldEqual(1);
            }

            [Test]
            public void the_service_should_have_thrown_an_other_message_that_is_an_error()
            {
                _errors.Count().ShouldEqual(1);
            }
        }
        [TestFixture]
        public class when_using_an_instance_of_the_StatLightService_that_is_initialized_with_filtering_options : using_a_random_temp_file_for_testing
        {
            private const string _tagFilter = "TEST";
            IStatLightService _statLightService;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                var config = new ClientTestRunConfiguration { TagFilter = _tagFilter };

                _statLightService = new StatLightService(new NullLogger(), TestEventAggregator, PathToTempXapFile, config, MockServerTestRunConfiguration);
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
                typeof(FileNotFoundException).ShouldBeThrownBy(() => new StatLightService(new NullLogger(), TestEventAggregator, "missingFile", ClientTestRunConfiguration.CreateDefault(), MockServerTestRunConfiguration));
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

                TestEventAggregator
                    .AddListener<TestRunCompletedServerEvent>(o => wasSignaledTestComplete = true);

                SignalTestComplete(StatLightService, 0);

                wasSignaledTestComplete.ShouldBeTrue();
            }

            [Test]
            public void it_should_wait_for_all_post_messages_to_arrive_if_the_TestComplete_signal_comes_in_with_a_message_count_greater_than_the_currently_posted()
            {
                bool wasSignaledTestComplete = false;

                TestEventAggregator
                    .AddListener<TestRunCompletedServerEvent>(o => wasSignaledTestComplete = true);

                // Signal completion of the test with a total of 2 messages 
                // (that should have been posted to the server)
                SignalTestComplete(StatLightService, 2);
                wasSignaledTestComplete.ShouldBeFalse();

                // Post the first 1
                StatLightService.PostMessage(MessageFactory.Create<TraceClientEvent>());
                wasSignaledTestComplete.ShouldBeFalse();

                // Post the second one and the event should have been fired.
                StatLightService.PostMessage(MessageFactory.Create<TraceClientEvent>());
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
                TestEventAggregator
                    .AddListener<TestRunCompletedServerEvent>(o => wasSignaledTestComplete = true);

                StatLightService.PostMessage(MessageFactory.Create<TraceClientEvent>());
                SignalTestComplete(StatLightService, 1);
                System.Threading.Thread.Sleep(10);

                wasSignaledTestComplete.ShouldBeTrue();
                //TestCompletedArgs.ShouldNotBeNull();
                //TestCompletedArgs.TotalClientMessageSentCount.ShouldEqual(1);

                wasSignaledTestComplete = false;

                StatLightService.PostMessage(MessageFactory.Create<TraceClientEvent>());
                SignalTestComplete(StatLightService, 1);
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

            protected override void Before_all_tests()
            {
                _messagesSent = 0;
                base.Before_all_tests();
            }

            protected void PostMessage(ClientEvent message)
            {
                StatLightService.PostMessage(message.Serialize().ToStream());
                _messagesSent++;
            }

            protected override int PostMessagesToService()
            {
                var otherMessages = new[]
                {
                    new TestExecutionMethodFailedClientEvent(),
                    new TestExecutionMethodFailedClientEvent(),
                    new TestExecutionMethodFailedClientEvent(),
                };


                var scenarioResults = new ClientEvent[]
                {
                    new TestExecutionMethodPassedClientEvent(),
                    new TestExecutionMethodFailedClientEvent(),
                    new TestExecutionMethodPassedClientEvent(),
                    new TestExecutionMethodPassedClientEvent(),
                    new TestExecutionMethodPassedClientEvent(),
                    new TestExecutionMethodPassedClientEvent(),
                    new TestExecutionMethodFailedClientEvent(),
                    new TestExecutionMethodFailedClientEvent(),
                };

                foreach (var message in scenarioResults)
                    PostMessage(message);

                foreach (var outcome in scenarioResults)
                    PostMessage(outcome);

                return _messagesSent;
            }

            [Test]
            public void should_have_received_the_TestCompletedArgs()
            {
                WasTestCompleteSignalSent.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class when_an_event_is_publshed_to_the_StatLightSerivce : with_an_instance_of_the_StatLightService
        {
            private bool _messageWasFired;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                TestEventAggregator.AddListener<MessageReceivedFromClientServerEvent>(() => _messageWasFired = true);

                base.StatLightService.PostMessage("hello world".ToStream());
            }

            [Test]
            public void Should_have_received_the_message_posted_event()
            {
                _messageWasFired.ShouldBeTrue();
            }
        }
    }
}