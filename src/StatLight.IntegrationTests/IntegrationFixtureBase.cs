using System;
using StatLight.Core.Tests;
using StatLight.IntegrationTests.ProviderTests;

namespace StatLight.IntegrationTests
{
    using System.IO;
    using StatLight.Core.Common;
    using StatLight.Core.Runners;
    using StatLight.Core.WebServer;
    using StatLight.Core.WebServer.XapHost;
    using StatLight.Core.Reporting;
    using StatLight.Core.Events.Aggregation;

    public abstract class IntegrationFixtureBase : FixtureBase
    {
        readonly StatLightRunnerFactory _statLightRunnerFactory = new StatLightRunnerFactory();
        private string _pathToIntegrationTestXap;
        private readonly ILogger _testLogger;

        protected IntegrationFixtureBase()
        {
            _testLogger = new ConsoleLogger(LogChatterLevels.Full);
            _pathToIntegrationTestXap = TestXapFileLocations.SilverlightIntegrationTests;
        }

        public string PathToIntegrationTestXap
        {
            get { return _pathToIntegrationTestXap; }
            set
            {
                if (File.Exists(value))
                    _pathToIntegrationTestXap = value;
                else
                    throw new FileNotFoundException("test xap file not found...[{0}]".FormatWith(value));
            }
        }

        protected IEventAggregator EventAggregator { get { return _statLightRunnerFactory.EventAggregator; } }

        protected TestReport TestReport { get; private set; }
        private IRunner Runner { get; set; }
        protected abstract ClientTestRunConfiguration ClientTestRunConfiguration { get; }

        protected override void Because()
        {
            base.Because();

            var xapReadItems = new Core.WebServer.XapInspection.XapReader(_testLogger).GetTestAssembly(_pathToIntegrationTestXap);
            xapReadItems.DebugWrite(_testLogger);

            var microsoftTestingFrameworkVersion = xapReadItems.MicrosoftSilverlightTestingFrameworkVersion ?? MicrosoftTestingFrameworkVersion.March2010;

            var xapHostFileLoaderFactory = new XapHostFileLoaderFactory(_testLogger);
            XapHostType mapToXapHostType = xapHostFileLoaderFactory.MapToXapHostType(ClientTestRunConfiguration.UnitTestProviderType, microsoftTestingFrameworkVersion);
            var serverTestRunConfiguration = new ServerTestRunConfiguration(this._testLogger, xapHostFileLoaderFactory, mapToXapHostType, xapReadItems)
            {
                DialogSmackDownElapseMilliseconds = 500,
            };

            bool showTestingBrowserHost = false;

            if (microsoftTestingFrameworkVersion == MicrosoftTestingFrameworkVersion.March2010)
                showTestingBrowserHost = true;

            Runner = _statLightRunnerFactory.CreateOnetimeConsoleRunner(_testLogger, _pathToIntegrationTestXap, ClientTestRunConfiguration, serverTestRunConfiguration, showTestingBrowserHost);
            TestReport = Runner.Run();
        }

        protected override void After_all_tests()
        {
            base.After_all_tests();

            Runner.Dispose();
            Runner = null;
        }

    }
}