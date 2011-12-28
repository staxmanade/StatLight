
using StatLight.Core.Common.Logging;

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
    using TinyIoC;

    public abstract class IntegrationFixtureBase : FixtureBase
    {
        private StatLightRunnerFactory _statLightRunnerFactory;
        private readonly ILogger _testLogger;
        private EventAggregator _eventSubscriptionManager;
        private TinyIoCContainer _ioc;

        protected IntegrationFixtureBase()
        {
            _testLogger = new ConsoleLogger(LogChatterLevels.Full);
        }

        protected abstract string GetTestXapPath();

        private string GetTestXapPathInternal()
        {
            var value = GetTestXapPath();

            if (File.Exists(value))
                return value;

            throw new FileNotFoundException("test xap file not found...[{0}]".FormatWith(value));
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
                    return MicrosoftTestingFrameworkVersion.MSTest2010April;
                return null;
            }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            System.Diagnostics.Debug.Assert(ClientTestRunConfiguration != null);

            var options = new InputOptions()
                .SetUnitTestProviderType(ClientTestRunConfiguration.UnitTestProviderType)
                .SetXapPaths(new[] { GetTestXapPathInternal()})
                .SetMicrosoftTestingFrameworkVersion(MSTestVersion)
                .SetMethodsToTest(ClientTestRunConfiguration.MethodsToTest)
                .SetTagFilters(ClientTestRunConfiguration.TagFilter)
            ;

            _ioc = BootStrapper.Initialize(options, _testLogger);

            _eventSubscriptionManager = _ioc.Resolve<EventAggregator>();
            _statLightRunnerFactory = new StatLightRunnerFactory(_testLogger, _ioc);

            var currentStatLightConfiguration = _ioc.Resolve<ICurrentStatLightConfiguration>();
            StatLightConfiguration statLightConfiguration = currentStatLightConfiguration.Current;
            _testLogger.Debug("Setting up xaphost {0}".FormatWith(statLightConfiguration.Server.XapHostType));

            Runner = _statLightRunnerFactory.CreateOnetimeConsoleRunner(statLightConfiguration);
        }

        protected override void Because()
        {
            base.Because();

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
