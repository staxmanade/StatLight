using System.Linq;
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
            IWebServer webServer;
            List<IBrowserFormHost> browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                showTestingBrowserHost,
                statLightConfiguration,
                out statLightService,
                out webServer,
                out browserFormHost);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);

            IRunner runner = new ContinuousConsoleRunner(logger, _eventAggregator, statLightConfiguration.Server.XapToTestPath, statLightService, webServer, browserFormHost.First());
            return runner;
        }

        public IRunner CreateTeamCityRunner(StatLightConfiguration statLightConfiguration)
        {
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            ILogger logger = new NullLogger();

            StatLightService statLightService;
            IWebServer webServer;
            List<IBrowserFormHost> browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                false,
                statLightConfiguration,
                out statLightService,
                out webServer,
                out browserFormHost);

            var teamCityTestResultHandler = new TeamCityTestResultHandler(new ConsoleCommandWriter(), statLightConfiguration.Server.XapToTestPath);
            _eventAggregator.AddListener(teamCityTestResultHandler);
            IRunner runner = new TeamCityRunner(new NullLogger(), _eventAggregator, webServer, browserFormHost, teamCityTestResultHandler);

            return runner;
        }

        public IRunner CreateOnetimeConsoleRunner(ILogger logger, StatLightConfiguration statLightConfiguration, bool showTestingBrowserHost)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            StatLightService statLightService;
            IWebServer webServer;
            List<IBrowserFormHost> browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                showTestingBrowserHost,
                statLightConfiguration,
                out statLightService,
                out webServer,
                out browserFormHost);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);

            IRunner runner = new OnetimeRunner(logger, _eventAggregator, webServer, browserFormHost);
            return runner;
        }

        public IRunner CreateWebServerOnlyRunner(ILogger logger, StatLightConfiguration statLightConfiguration)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            var location = new WebServerLocation();

            StatLightService statLightService;
            var webServer = CreateWebServer(logger, statLightConfiguration, location, out statLightService);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);
            SetupDebugClientEventListener(logger);
            IRunner runner = new WebServerOnlyRunner(logger, _eventAggregator, webServer, location.TestPageUrl);

            return runner;
        }

        private IWebServer CreateWebServer(ILogger logger, StatLightConfiguration statLightConfiguration, WebServerLocation location, out StatLightService statLightService)
        {
            statLightService = new StatLightService(logger, _eventAggregator, statLightConfiguration.Client, statLightConfiguration.Server);

            return new StatLightServiceHost(logger, statLightService, location.BaseUrl);
        }

        private void BuildAndReturnWebServiceAndBrowser(
            ILogger logger,
            bool showTestingBrowserHost,
            StatLightConfiguration statLightConfiguration,
            out StatLightService statLightService,
            out IWebServer webServer,
            out List<IBrowserFormHost> browserFormHosts)
        {
            ClientTestRunConfiguration clientTestRunConfiguration = statLightConfiguration.Client;
            ServerTestRunConfiguration serverTestRunConfiguration = statLightConfiguration.Server;

            var location = new WebServerLocation();
            var debugAssertMonitorTimer = new TimerWrapper(serverTestRunConfiguration.DialogSmackDownElapseMilliseconds);
            var dialogMonitors = new List<IDialogMonitor>
			{
				new DebugAssertMonitor(logger),
				new MessageBoxMonitor(logger),
			};
            var dialogMonitorRunner = new DialogMonitorRunner(logger, _eventAggregator, debugAssertMonitorTimer, dialogMonitors);
            SetupDebugClientEventListener(logger);
            webServer = CreateWebServer(logger, statLightConfiguration, location, out statLightService);

            // The new March/April 2010 will fail in the "minimized mode" 
            //TODO figure out how to not get the errors when these are minimized
            if (serverTestRunConfiguration.XapHostType == XapHostType.MSTestMarch2010 ||
                serverTestRunConfiguration.XapHostType == XapHostType.MSTestApril2010)
                showTestingBrowserHost = true;

            browserFormHosts = Enumerable.Range(1, clientTestRunConfiguration.NumberOfBrowserHosts)
                .Select(browserI => new BrowserFormHost(logger, location.TestPageUrl, showTestingBrowserHost, dialogMonitorRunner))
                .Cast<IBrowserFormHost>()
                .ToList();

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
