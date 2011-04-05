using System;
using System.Collections.ObjectModel;
using System.IO;
using Moq;
using NUnit.Framework;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.WebServer.XapHost;
using System.Collections.Generic;

namespace StatLight.Core.Tests
{
    public class FixtureBase
    {
        public FixtureBase()
        {
            TestLogger = new NullLogger();
        }

        public ILogger TestLogger { get; set; }

        private EventAggregator _eventSubscriptionManager;

        public IEventSubscriptionManager TestEventSubscriptionManager { get { return _eventSubscriptionManager; } }

        protected IEventPublisher TestEventPublisher { get { return _eventSubscriptionManager; } }

        [TestFixtureSetUp]
        public void SetupContext()
        {
            _eventSubscriptionManager = new EventAggregator(TestLogger);

            Before_all_tests();
            Because();
        }

        [TestFixtureTearDown]
        public void TearDownContext()
        {
            After_all_tests();
        }

        protected virtual void Before_all_tests()
        {
        }

        protected virtual void Because()
        {
        }

        protected virtual void After_all_tests()
        {
        }
    }


    public class using_a_random_temp_file_for_testing : FixtureBase
    {
        private string _pathToTempXapFile;
        private ServerTestRunConfiguration _mockServerTestRunConfiguration;

        protected string PathToTempXapFile
        {
            get { return _pathToTempXapFile; }
        }

        public ServerTestRunConfiguration MockServerTestRunConfiguration
        {
            get { return _mockServerTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            _pathToTempXapFile = Path.GetTempFileName();

            Func<byte[]> xapToTestFactory = () => File.ReadAllBytes(_pathToTempXapFile);

            using (var writer = File.CreateText(_pathToTempXapFile))
            {
                writer.Close();
            }
            var mockXapHostFileLoaderFactory = new Mock<XapHostFileLoaderFactory>(TestLogger);
            mockXapHostFileLoaderFactory
                .Setup(s => s.LoadXapHostFor(It.IsAny<XapHostType>()))
                .Returns(new byte[] { 0, 1, 1, 2, 3, 1, });
            _mockServerTestRunConfiguration = new ServerTestRunConfiguration(() => new byte[] { 1, 2 }, 500, _pathToTempXapFile, XapHostType.MSTestNovember2009, "", true, false, isPhoneRun: false);

            Assert.IsTrue(File.Exists(_pathToTempXapFile));
        }


        protected override void After_all_tests()
        {
            base.After_all_tests();
            if (File.Exists(_pathToTempXapFile))
                File.Delete(_pathToTempXapFile);
        }

        protected virtual void replace_test_file()
        {
            File.Delete(_pathToTempXapFile);

            using (var writer = File.CreateText(_pathToTempXapFile))
            {
                writer.Close();
            }
        }

        protected ClientTestRunConfiguration CreateTestDefaultClinetTestRunConfiguraiton()
        {
            return new ClientTestRunConfiguration(UnitTestProviderType.MSTest, new Collection<string>(),
                                                                         string.Empty, 1, StatLight.Core.WebBrowser.WebBrowserType.SelfHosted, false, string.Empty, new List<string>());
        }
    }

}
