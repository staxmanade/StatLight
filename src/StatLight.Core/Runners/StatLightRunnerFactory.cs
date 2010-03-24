

using StatLight.Client.Harness.Events;

namespace StatLight.Core.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
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
        internal IEventAggregator EventAggregator = new EventAggregator(new SynchronizationContext());
        private BrowserCommunicationTimeoutMonitor _browserCommunicationTimeoutMonitor;
        private ConsoleResultHandler _consoleResultHandler;
        private Action<DebugClientEvent> _debugEventListener;
        public IRunner CreateContinuousTestRunner(ILogger logger, string xapPath, ClientTestRunConfiguration clientTestRunConfiguration, bool showTestingBrowserHost, ServerTestRunConfiguration serverTestRunConfiguration)
        {
            StatLightService statLightService;
            StatLightServiceHost statLightServiceHost;
            BrowserFormHost browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                xapPath,
                showTestingBrowserHost,
                clientTestRunConfiguration,
                serverTestRunConfiguration,
                out statLightService,
                out statLightServiceHost,
                out browserFormHost);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);

            IRunner runner = new ContinuousConsoleRunner(logger, EventAggregator, xapPath, statLightService, statLightServiceHost, browserFormHost);
            return runner;
        }

        public IRunner CreateTeamCityRunner(string xapPath, ClientTestRunConfiguration clientTestRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration)
        {
            ILogger logger = new NullLogger();

            StatLightService statLightService;
            StatLightServiceHost statLightServiceHost;
            BrowserFormHost browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                xapPath,
                false,
                clientTestRunConfiguration,
                serverTestRunConfiguration,
                out statLightService,
                out statLightServiceHost,
                out browserFormHost);

            var teamCityTestResultHandler = new TeamCityTestResultHandler(new ConsoleCommandWriter(), xapPath);
            EventAggregator.AddListener(teamCityTestResultHandler);
            IRunner runner = new TeamCityRunner(new NullLogger(), EventAggregator, statLightServiceHost, browserFormHost, teamCityTestResultHandler);

            return runner;
        }

        public IRunner CreateOnetimeConsoleRunner(ILogger logger, string xapPath, ClientTestRunConfiguration clientTestRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration, bool showTestingBrowserHost)
        {
            StatLightService statLightService;
            StatLightServiceHost statLightServiceHost;
            BrowserFormHost browserFormHost;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                xapPath,
                showTestingBrowserHost,
                clientTestRunConfiguration,
                serverTestRunConfiguration,
                out statLightService,
                out statLightServiceHost,
                out browserFormHost);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);

            IRunner runner = new OnetimeRunner(logger, EventAggregator, statLightServiceHost, browserFormHost);
            return runner;
        }

        public IRunner CreateWebServerOnlyRunner(ILogger logger, string xapPath, ClientTestRunConfiguration clientTestRunConfiguration, ServerTestRunConfiguration serverTestRunConfiguration)
        {
            var location = new WebServerLocation();

            var statLightService = new StatLightService(logger, EventAggregator, xapPath, clientTestRunConfiguration, serverTestRunConfiguration);
            var statLightServiceHost = new StatLightServiceHost(logger, statLightService, location.BaseUrl);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);
            SetupDebugClientEventListener(logger);
            IRunner runner = new WebServerOnlyRunner(logger, EventAggregator, statLightServiceHost, location.TestPageUrl);

            return runner;
        }

        private void BuildAndReturnWebServiceAndBrowser(
            ILogger logger,
            string xapPath,
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
            var dialogMonitorRunner = new DialogMonitorRunner(logger, EventAggregator, debugAssertMonitorTimer, dialogMonitors);
            SetupDebugClientEventListener(logger);
            statLightService = new StatLightService(logger, EventAggregator, xapPath, clientTestRunConfiguration, serverTestRunConfiguration);
            statLightServiceHost = new StatLightServiceHost(logger, statLightService, location.BaseUrl);
            browserFormHost = new BrowserFormHost(logger, location.TestPageUrl, showTestingBrowserHost, dialogMonitorRunner);

            StartupBrowserCommunicationTimeoutMonitor(new TimeSpan(0, 0, 5, 0));
        }

        private void StartupBrowserCommunicationTimeoutMonitor(TimeSpan maxTimeAllowedBeforeCommErrorSent)
        {
            if (_browserCommunicationTimeoutMonitor == null)
                _browserCommunicationTimeoutMonitor = new BrowserCommunicationTimeoutMonitor(EventAggregator, new TimerWrapper(3000), maxTimeAllowedBeforeCommErrorSent);
        }


        private void CreateAndAddConsoleResultHandlerToEventAggregator(ILogger logger)
        {
            if (_consoleResultHandler == null)
            {
                _consoleResultHandler = new ConsoleResultHandler(logger);
                EventAggregator.AddListener(_consoleResultHandler);
            }
        }

        private void SetupDebugClientEventListener(ILogger logger)
        {
            if (_debugEventListener == null)
            {
                _debugEventListener = e => logger.Debug(e.Message);
                EventAggregator.AddListener<DebugClientEvent>(_debugEventListener);
            }
        }

    }
}
