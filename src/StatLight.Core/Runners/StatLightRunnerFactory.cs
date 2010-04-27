using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.Runners
{
    using System;
    using System.Collections.Generic;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Configuration;
    using StatLight.Core.Common;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Monitoring;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.Reporting.Providers.TeamCity;
    using StatLight.Core.Timing;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;

    public class StatLightRunnerFactory
    {
        private readonly IEventAggregator _eventAggregator;
        private BrowserCommunicationTimeoutMonitor _browserCommunicationTimeoutMonitor;
        private ConsoleResultHandler _consoleResultHandler;
        private Action<DebugClientEvent> _debugEventListener;

        public StatLightRunnerFactory()
            : this(new EventAggregator())
        {}

        public StatLightRunnerFactory(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public IRunner CreateContinuousTestRunner(ILogger logger, StatLightConfiguration statLightConfiguration, bool showTestingBrowserHost)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            StatLightService statLightService;
            StatLightServiceHost statLightServiceHost;
            BrowserFormHost browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                showTestingBrowserHost,
                statLightConfiguration.Client,
                statLightConfiguration.Server,
                out statLightService,
                out statLightServiceHost,
                out browserFormHost);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);

            IRunner runner = new ContinuousConsoleRunner(logger, _eventAggregator, statLightConfiguration.Server.XapToTestPath, statLightService, statLightServiceHost, browserFormHost);
            return runner;
        }

        public IRunner CreateTeamCityRunner(StatLightConfiguration statLightConfiguration)
        {
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            ILogger logger = new NullLogger();

            StatLightService statLightService;
            StatLightServiceHost statLightServiceHost;
            BrowserFormHost browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                false,
                statLightConfiguration.Client,
                statLightConfiguration.Server,
                out statLightService,
                out statLightServiceHost,
                out browserFormHost);

            var teamCityTestResultHandler = new TeamCityTestResultHandler(new ConsoleCommandWriter(), statLightConfiguration.Server.XapToTestPath);
            _eventAggregator.AddListener(teamCityTestResultHandler);
            IRunner runner = new TeamCityRunner(new NullLogger(), _eventAggregator, statLightServiceHost, browserFormHost, teamCityTestResultHandler);

            return runner;
        }

        public IRunner CreateOnetimeConsoleRunner(ILogger logger, StatLightConfiguration statLightConfiguration, bool showTestingBrowserHost)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            StatLightService statLightService;
            StatLightServiceHost statLightServiceHost;
            BrowserFormHost browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                showTestingBrowserHost,
                statLightConfiguration.Client,
                statLightConfiguration.Server,
                out statLightService,
                out statLightServiceHost,
                out browserFormHost);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);

            IRunner runner = new OnetimeRunner(logger, _eventAggregator, statLightServiceHost, browserFormHost);
            return runner;
        }

        public IRunner CreateWebServerOnlyRunner(ILogger logger, StatLightConfiguration statLightConfiguration)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            var location = new WebServerLocation();

            var statLightService = new StatLightService(logger, _eventAggregator, statLightConfiguration.Client, statLightConfiguration.Server);
            var statLightServiceHost = new StatLightServiceHost(logger, statLightService, location.BaseUrl);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);
            SetupDebugClientEventListener(logger);
            IRunner runner = new WebServerOnlyRunner(logger, _eventAggregator, statLightServiceHost, location.TestPageUrl);

            return runner;
        }

        private void BuildAndReturnWebServiceAndBrowser(
            ILogger logger,
            bool showTestingBrowserHost,
            ClientTestRunConfiguration clientTestRunConfiguration,
            ServerTestRunConfiguration serverTestRunConfiguration,
            out StatLightService statLightService,
            out StatLightServiceHost statLightServiceHost,
            out BrowserFormHost browserFormHost)
        {

            var location = new WebServerLocation();
            var debugAssertMonitorTimer = new TimerWrapper(serverTestRunConfiguration.DialogSmackDownElapseMilliseconds);
            var dialogMonitors = new List<IDialogMonitor>
			{
				new DebugAssertMonitor(logger),
				new MessageBoxMonitor(logger),
			};
            var dialogMonitorRunner = new DialogMonitorRunner(logger, _eventAggregator, debugAssertMonitorTimer, dialogMonitors);
            SetupDebugClientEventListener(logger);
            statLightService = new StatLightService(logger, _eventAggregator, clientTestRunConfiguration, serverTestRunConfiguration);
            statLightServiceHost = new StatLightServiceHost(logger, statLightService, location.BaseUrl);

            // The new March/April 2010 will fail in the "minimized mode" 
            //TODO figure out how to not get the errors when these are minimized
            if (serverTestRunConfiguration.XapHostType == XapHostType.MSTestMarch2010 ||
                serverTestRunConfiguration.XapHostType == XapHostType.MSTestApril2010)
                showTestingBrowserHost = true;

            browserFormHost = new BrowserFormHost(logger, location.TestPageUrl, showTestingBrowserHost, dialogMonitorRunner);

            StartupBrowserCommunicationTimeoutMonitor(new TimeSpan(0, 0, 5, 0));
        }

        private void StartupBrowserCommunicationTimeoutMonitor(TimeSpan maxTimeAllowedBeforeCommErrorSent)
        {
            if (_browserCommunicationTimeoutMonitor == null)
                _browserCommunicationTimeoutMonitor = new BrowserCommunicationTimeoutMonitor(_eventAggregator, new TimerWrapper(3000), maxTimeAllowedBeforeCommErrorSent);
        }


        private void CreateAndAddConsoleResultHandlerToEventAggregator(ILogger logger)
        {
            if (_consoleResultHandler == null)
            {
                _consoleResultHandler = new ConsoleResultHandler(logger);
                _eventAggregator.AddListener(_consoleResultHandler);
            }
        }

        private void SetupDebugClientEventListener(ILogger logger)
        {
            ((EventAggregator) _eventAggregator).Logger = logger;
            if (_debugEventListener == null)
            {
                _debugEventListener = e => logger.Debug(e.Message);
                _eventAggregator.AddListener(_debugEventListener);
            }
        }

    }
}
