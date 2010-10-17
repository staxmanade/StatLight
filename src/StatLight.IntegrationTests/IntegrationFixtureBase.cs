
namespace StatLight.IntegrationTests
{
    using System.IO;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Reporting;
    using StatLight.Core.Runners;
    using StatLight.Core.WebServer.XapHost;
    using StatLight.IntegrationTests.ProviderTests;

    public abstract class IntegrationFixtureBase : FixtureBase
    {
        private StatLightRunnerFactory _statLightRunnerFactory;
        private string _pathToIntegrationTestXap;
        private readonly ILogger _testLogger;
        private EventAggregator _eventAggregator;

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

        protected IEventAggregator EventAggregator { get { return _eventAggregator; } }

        protected TestReport TestReport { get; private set; }
        private IRunner Runner { get; set; }
        protected abstract ClientTestRunConfiguration ClientTestRunConfiguration { get; }
        protected virtual MicrosoftTestingFrameworkVersion? MSTestVersion
        {
            get
            {
                if (ClientTestRunConfiguration.UnitTestProviderType == UnitTestProviderType.MSTest)
                    return MicrosoftTestingFrameworkVersion.April2010;
                return null;
            }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();
            _eventAggregator = new EventAggregator();
            _statLightRunnerFactory = new StatLightRunnerFactory(_eventAggregator);
        }

        protected override void Because()
        {
            base.Because();

            var statLightConfigurationFactory = new StatLightConfigurationFactory(_testLogger);

            var statLightConfiguration = statLightConfigurationFactory.GetStatLightConfiguration(
                ClientTestRunConfiguration.UnitTestProviderType,
                _pathToIntegrationTestXap,
                MSTestVersion,
                ClientTestRunConfiguration.MethodsToTest,
                ClientTestRunConfiguration.TagFilter,
                1,
                false,
                "", StatLight.Core.WebBrowser.WebBrowserType.SelfHosted,
                forceBrowserStart:true);

            bool showTestingBrowserHost = statLightConfiguration.Server.XapHostType == XapHostType.MSTestApril2010;
            _testLogger.Debug("Setting up xaphost {0}".FormatWith(statLightConfiguration.Server.XapHostType));
            Runner = _statLightRunnerFactory.CreateOnetimeConsoleRunner(_testLogger, statLightConfiguration, showTestingBrowserHost);

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