
namespace StatLight.IntegrationTests
{
    using System.IO;
    using System.Linq;
    using StatLight.Core;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events;
    using StatLight.Core.Reporting;
    using StatLight.Core.Runners;
    using StatLight.Core.WebServer.XapHost;
    using StatLight.IntegrationTests.ProviderTests;
    using TinyIoC;

    public abstract class IntegrationFixtureBase : FixtureBase
    {
        private StatLightRunnerFactory _statLightRunnerFactory;
        private string _pathToIntegrationTestXap;
        private readonly ILogger _testLogger;
        private EventAggregator _eventSubscriptionManager;
        private TinyIoCContainer _ioc;

        protected IntegrationFixtureBase()
        {
            _testLogger = new ConsoleLogger(LogChatterLevels.Full);
            _pathToIntegrationTestXap = TestXapFileLocations.SilverlightIntegrationTests;
            _ioc = BootStrapper.Initialize(new InputOptions(), _testLogger);
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

        protected IEventSubscriptionManager EventSubscriptionManager { get { return _eventSubscriptionManager; } }
        protected IEventPublisher EventPublisher { get { return _eventSubscriptionManager; } }

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
            _eventSubscriptionManager = IoC.Resolve<EventAggregator>();

            _statLightRunnerFactory = new StatLightRunnerFactory(_testLogger, _ioc);
        }

        protected TinyIoCContainer IoC { get { return _ioc; } }

        protected override void Because()
        {
            base.Because();

            var options = new InputOptions()
                .SetUnitTestProviderType(ClientTestRunConfiguration.UnitTestProviderType)
                .SetXapPaths(new[] {_pathToIntegrationTestXap})
                .SetMicrosoftTestingFrameworkVersion(MSTestVersion)
                .SetMethodsToTest(ClientTestRunConfiguration.MethodsToTest)
                .SetTagFilters(ClientTestRunConfiguration.TagFilter)
            ;


            var statLightConfigurationFactory = new StatLightConfigurationFactory(_testLogger, options);

            StatLightConfiguration statLightConfiguration = statLightConfigurationFactory.GetConfigurations().Single();
            _testLogger.Debug("Setting up xaphost {0}".FormatWith(statLightConfiguration.Server.XapHostType));
            Runner = _statLightRunnerFactory.CreateOnetimeConsoleRunner(statLightConfiguration);

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