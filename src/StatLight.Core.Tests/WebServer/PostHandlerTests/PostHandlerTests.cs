using System.Collections.Generic;
using System.IO;
using Moq;
using NUnit.Framework;
using StatLight.Core.Configuration;
using StatLight.Core.Events;
using StatLight.Core.Serialization;
using StatLight.Core.WebBrowser;
using StatLight.Core.WebServer;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.Tests.WebServer.PostHandlerTests
{
    public class PostHandlerTestsBase : FixtureBase
    {
        protected PostHandler PostHandler { get; set; }
        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            var mockCurrentStatLightConfiguration = new Mock<ICurrentStatLightConfiguration>();
            var statlightConfiguration = new StatLightConfiguration(
                new ClientTestRunConfiguration(
                    unitTestProviderType: UnitTestProviderType.MSTest,
                    methodsToTest: new List<string>(),
                    tagFilters: string.Empty,
                    numberOfBrowserHosts: 1,
                    webBrowserType: WebBrowserType.SelfHosted,
                    entryPointAssembly: null,
                    windowGeometry: new WindowGeometry()
                ),
                new ServerTestRunConfiguration(xapHost: () => new byte[0],
                    xapToTest: string.Empty,
                    xapHostType: XapHostType.MSTest2008December,
                    queryString: string.Empty,
                    forceBrowserStart: true,
                    windowGeometry: new WindowGeometry()
                )
            );

            mockCurrentStatLightConfiguration.Setup(s => s.Current).Returns(statlightConfiguration);

            var responseFactory = new ResponseFactory(mockCurrentStatLightConfiguration.Object);
            PostHandler = new PostHandler(TestLogger, TestEventPublisher, mockCurrentStatLightConfiguration.Object, responseFactory);
        }

        protected void HandleMessage(object message)
        {
            Stream stream = message.Serialize().ToStream();
            string unknownPostData;
            PostHandler.TryHandle(stream, out unknownPostData);
            if (!string.IsNullOrEmpty(unknownPostData))
            {
                throw new AssertionException("Could not post object of type [{0}] to the PostHanlder posted as [{1}]".FormatWith(message.GetType(), unknownPostData));
            }

        }
    }

    public class Double_test_run_should_signal_when_complete : PostHandlerTestsBase
    {
        private int _testRunCompleteMessagesSentCount = 0;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            TestEventSubscriptionManager.AddListenerAction<TestRunCompletedServerEvent>(msg =>
            {
                _testRunCompleteMessagesSentCount++;
            });

            HandleMessage(new TestExecutionMethodBeginClientEvent());
            HandleMessage(new TestExecutionMethodPassedClientEvent());
            HandleMessage(new SignalTestCompleteClientEvent() { BrowserInstanceId = 0, TotalMessagesPostedCount = 3 });

            _testRunCompleteMessagesSentCount.ShouldEqual(1);

            HandleMessage(new TestExecutionMethodBeginClientEvent());
            _testRunCompleteMessagesSentCount.ShouldEqual(1);
            HandleMessage(new TestExecutionMethodPassedClientEvent());
            _testRunCompleteMessagesSentCount.ShouldEqual(1);
            HandleMessage(new SignalTestCompleteClientEvent() { BrowserInstanceId = 0, TotalMessagesPostedCount = 3 });
        }

        [Test]
        public void Total_completedMessagesSentCount_should_be_correct()
        {
            _testRunCompleteMessagesSentCount.ShouldEqual(2);
        }
    }
}