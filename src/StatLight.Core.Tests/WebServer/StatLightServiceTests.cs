using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using Moq;
using NUnit.Framework;
using StatLight.Client.Harness.Events;
using StatLight.Core.Common;
using StatLight.Core.Events;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Serialization;
using StatLight.Core.Tests.Mocks;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebBrowser;
using StatLight.Core.WebServer;
using StatLight.Core.Configuration;
using StatLight.Core.Reporting;
using StatLight.Core.WebServer.Host;

namespace StatLight.Core.Tests.WebServer
{
    internal class TestServiceWrapper : IStatLightService
    {

        private TestServiceEngine _testServiceEngine;

        private WebClient _webClient;
        private readonly Func<byte[]> _xapToTestFactory;
        private int _port;

        public TestServiceWrapper(ILogger logger, IEventAggregator eventAggregator, ClientTestRunConfiguration clientTestRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration)
        {
            var consoleLogger = new ConsoleLogger(LogChatterLevels.Full);
            _xapToTestFactory = () => new byte[] { 0, 1, 2, 3, 4 };
            var hostXap = new byte[] { 5, 4, 2, 1, 4 };
            var responseFactory = new ResponseFactory(_xapToTestFactory, hostXap, clientTestRunConfiguration);


            PostHandler postHandler = new PostHandler(logger, eventAggregator, clientTestRunConfiguration);
            _port = 34245;
            _testServiceEngine = new TestServiceEngine(consoleLogger, "lodalhost", _port, responseFactory, postHandler);
            _webClient = new WebClient();

        }

        public string TagFilters
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public Stream GetCrossDomainPolicy()
        {
            throw new NotImplementedException();
        }

        public void PostMessage(Stream stream)
        {
            byte[] data = stream.StreamToString().ToByteArray();
            var url = "http://{0}:{1}/2".FormatWith("lodalhost", _port, StatLightServiceRestApi.PostMessage);
            Stream openWrite = _webClient.OpenWrite(url);
            openWrite.Write(data, 0, data.Length);
            openWrite.Close();
        }

        public Stream GetTestXap()
        {
            throw new NotImplementedException();
        }

        public Stream GetHtmlTestPage()
        {
            throw new NotImplementedException();
        }

        public Stream GetTestPageHostXap()
        {
            throw new NotImplementedException();
        }

        public ClientTestRunConfiguration GetTestRunConfiguration()
        {
            throw new NotImplementedException();
        }
    }
    namespace StatLightServiceTests
    {

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
                var clientConfig = new ClientTestRunConfiguration(UnitTestProviderType.MSTest, new List<string>(), "", 1, "test", WebBrowserType.SelfHosted);
                var postHandler = new PostHandler(base.TestLogger, base.TestEventAggregator, clientConfig);
                _statLightService = new StatLightService(new NullLogger(), base.CreateTestDefaultClinetTestRunConfiguraiton(), serverConfig, postHandler);
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

                TestResultAggregator = new TestResultAggregator(TestLogger, TestEventAggregator, "test");
                TestEventAggregator.AddListener(TestResultAggregator);

                TestEventAggregator
                    .AddListener<TestRunCompletedServerEvent>(e => WasTestCompleteSignalSent = true);


                TestEventAggregator
                    .AddListener<UnhandledExceptionClientEvent>(e => _errors.Add(e));

                var postCount = PostMessagesToService();

                SignalTestComplete(StatLightService, postCount + 1);
            }

            protected virtual int PostMessagesToService()
            {
                return 1;
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

                var config = new ClientTestRunConfiguration(UnitTestProviderType.MSTest, new Collection<string>(), _tagFilter, 1, "", StatLight.Core.WebBrowser.WebBrowserType.SelfHosted);

                _statLightService = new StatLightService(new NullLogger(), config, MockServerTestRunConfiguration, new Mock<IPostHandler>().Object);
            }

            [Test]
            public void should_be_able_to_get_the_startup_options()
            {
                _statLightService.GetTestRunConfiguration().ShouldNotBeNull();
            }
        }

        [TestFixture]
        public class when_testing_the_use_of_an_instance_of_the_StatLightService_it : with_an_instance_of_the_StatLightService
        {

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
                crossDomainPolicy.StreamToString().ShouldContain("cross-domain-policy");
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

                SignalTestComplete(StatLightService, 1);

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
                SignalTestComplete(StatLightService, 3);
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
                SignalTestComplete(StatLightService, 2);
                System.Threading.Thread.Sleep(10);

                wasSignaledTestComplete.ShouldBeTrue();
                //TestCompletedArgs.ShouldNotBeNull();
                //TestCompletedArgs.TotalClientMessageSentCount.ShouldEqual(1);

                wasSignaledTestComplete = false;

                StatLightService.PostMessage(MessageFactory.Create<TraceClientEvent>());
                SignalTestComplete(StatLightService, 2);
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
                _messagesSent++;
                StatLightService.PostMessage(message.Serialize().ToStream());
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