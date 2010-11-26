
using StatLight.Core.Common.Abstractions.Timing;
using StatLight.Core.WebServer.CustomHost;

namespace StatLight.Core.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Web;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Configuration;
    using StatLight.Core.Common;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Monitoring;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.Reporting.Providers.TeamCity;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;
    using StatLight.Core.WebServer.XapHost;

    public class StatLightRunnerFactory
    {
        private readonly IEventAggregator _eventAggregator;
        private BrowserCommunicationTimeoutMonitor _browserCommunicationTimeoutMonitor;
        private ConsoleResultHandler _consoleResultHandler;
        private Action<DebugClientEvent> _debugEventListener;

        public StatLightRunnerFactory() : this(new EventAggregator()) { }

        public StatLightRunnerFactory(IEventAggregator eventAggregator)
        {
            _eventAggregator = eventAggregator;
        }

        public IRunner CreateContinuousTestRunner(ILogger logger, StatLightConfiguration statLightConfiguration)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            IWebServer webServer;
            List<IWebBrowser> webBrowsers;
            IDialogMonitorRunner dialogMonitorRunner;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                statLightConfiguration.Server.ShowTestingBrowserHost,
                statLightConfiguration,
                out webServer,
                out webBrowsers,
                out dialogMonitorRunner);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);

            IRunner runner = new ContinuousConsoleRunner(logger, _eventAggregator, statLightConfiguration.Server.XapToTestPath, statLightConfiguration.Client, webServer, webBrowsers.First());
            return runner;
        }

        public IRunner CreateTeamCityRunner(StatLightConfiguration statLightConfiguration)
        {
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            ILogger logger = new NullLogger();
            IWebServer webServer;
            List<IWebBrowser> webBrowsers;
            IDialogMonitorRunner dialogMonitorRunner;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                false,
                statLightConfiguration,
                out webServer,
                out webBrowsers,
                out dialogMonitorRunner);

            var teamCityTestResultHandler = new TeamCityTestResultHandler(new ConsoleCommandWriter(), statLightConfiguration.Server.XapToTestPath);
            _eventAggregator.AddListener(teamCityTestResultHandler);
            IRunner runner = new TeamCityRunner(new NullLogger(), _eventAggregator, webServer, webBrowsers, teamCityTestResultHandler, statLightConfiguration.Server.XapToTestPath, dialogMonitorRunner);

            return runner;
        }

        public IRunner CreateOnetimeConsoleRunner(ILogger logger, StatLightConfiguration statLightConfiguration)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            IWebServer webServer;
            List<IWebBrowser> webBrowsers;
            IDialogMonitorRunner dialogMonitorRunner;

            BuildAndReturnWebServiceAndBrowser(
                logger,
                statLightConfiguration.Server.ShowTestingBrowserHost,
                statLightConfiguration,
                out webServer,
                out webBrowsers,
                out dialogMonitorRunner);

            CreateAndAddConsoleResultHandlerToEventAggregator(logger);
            IRunner runner = new OnetimeRunner(logger, _eventAggregator, webServer, webBrowsers, statLightConfiguration.Server.XapToTestPath, dialogMonitorRunner);
            return runner;
        }

        public IRunner CreateWebServerOnlyRunner(ILogger logger, StatLightConfiguration statLightConfiguration)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            var location = new WebServerLocation(logger);

            var webServer = CreateWebServer(logger, statLightConfiguration, location);
            CreateAndAddConsoleResultHandlerToEventAggregator(logger);
            SetupDebugClientEventListener(logger);
            IRunner runner = new WebServerOnlyRunner(logger, _eventAggregator, webServer, location.TestPageUrl, statLightConfiguration.Server.XapToTestPath);

            return runner;
        }

        private static void SetXapHostUrl(StatLightConfiguration statLightConfiguration, WebServerLocation location)
        {
            if (statLightConfiguration.Client.XapToTestUrl == null)
                statLightConfiguration.Client.XapToTestUrl = (location.BaseUrl + StatLightServiceRestApi.GetXapToTest);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private IWebServer CreateWebServer(ILogger logger, StatLightConfiguration statLightConfiguration, WebServerLocation location)
        {
            SetXapHostUrl(statLightConfiguration, location);

            var postHandler = new PostHandler(logger, _eventAggregator, statLightConfiguration.Client);

            //var statLightService = new StatLightService(logger, statLightConfiguration.Client, statLightConfiguration.Server, postHandler);
            //return new StatLightServiceHost(logger, statLightService, location.BaseUrl);

            int port = location.BaseUrl.Port;
            var responseFactory = new ResponseFactory(statLightConfiguration.Server.XapToTextFactory, statLightConfiguration.Server.HostXap, statLightConfiguration.Client);
            return new TestServiceEngine(logger, port, responseFactory, postHandler);

        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void BuildAndReturnWebServiceAndBrowser(
            ILogger logger,
            bool showTestingBrowserHost,
            StatLightConfiguration statLightConfiguration,
            out IWebServer webServer,
            out List<IWebBrowser> webBrowsers,
            out IDialogMonitorRunner dialogMonitorRunner)
        {
            ClientTestRunConfiguration clientTestRunConfiguration = statLightConfiguration.Client;
            ServerTestRunConfiguration serverTestRunConfiguration = statLightConfiguration.Server;

            var location = new WebServerLocation(logger);
            var debugAssertMonitorTimer = new TimerWrapper(serverTestRunConfiguration.DialogSmackDownElapseMilliseconds);
            SetupDebugClientEventListener(logger);
            webServer = CreateWebServer(logger, statLightConfiguration, location);

            showTestingBrowserHost = GetShowTestingBrowserHost(serverTestRunConfiguration, showTestingBrowserHost);

            webBrowsers = GetWebBrowsers(logger, location.TestPageUrl, clientTestRunConfiguration, showTestingBrowserHost, serverTestRunConfiguration.QueryString, statLightConfiguration.Server.ForceBrowserStart);

            dialogMonitorRunner = SetupDialogMonitorRunner(logger, webBrowsers, debugAssertMonitorTimer);

            StartupBrowserCommunicationTimeoutMonitor(new TimeSpan(0, 0, 5, 0));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "testPageUrlWithQueryString")]
        private static List<IWebBrowser> GetWebBrowsers(ILogger logger, Uri testPageUrl, ClientTestRunConfiguration clientTestRunConfiguration, bool showTestingBrowserHost, string queryString, bool forceBrowserStart)
        {
            var webBrowserType = clientTestRunConfiguration.WebBrowserType;
            var webBrowserFactory = new WebBrowserFactory(logger);
            var testPageUrlWithQueryString = new Uri(testPageUrl + "?" + queryString);
            logger.Debug("testPageUrlWithQueryString = " + testPageUrlWithQueryString);
            List<IWebBrowser> webBrowsers = Enumerable
                .Range(1, clientTestRunConfiguration.NumberOfBrowserHosts)
                .Select(browserI => webBrowserFactory.Create(webBrowserType, testPageUrlWithQueryString, showTestingBrowserHost, forceBrowserStart, clientTestRunConfiguration.NumberOfBrowserHosts > 1))
                .Cast<IWebBrowser>()
                .ToList();
            return webBrowsers;
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
            ((EventAggregator)_eventAggregator).Logger = logger;
            if (_debugEventListener == null)
            {
                _debugEventListener = e => logger.Debug(e.Message);
                _eventAggregator.AddListener(_debugEventListener);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IRunner CreateRemotelyHostedRunner(ILogger logger, StatLightConfiguration statLightConfiguration)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");

            ClientTestRunConfiguration clientTestRunConfiguration = statLightConfiguration.Client;
            ServerTestRunConfiguration serverTestRunConfiguration = statLightConfiguration.Server;

            var urlToTestPage = statLightConfiguration.Client.XapToTestUrl.ToUri();

            var location = new RemoteSiteOverriddenLocation(logger, urlToTestPage);
            var debugAssertMonitorTimer = new TimerWrapper(serverTestRunConfiguration.DialogSmackDownElapseMilliseconds);
            SetupDebugClientEventListener(logger);
            var webServer = CreateWebServer(logger, statLightConfiguration, location);

            var showTestingBrowserHost = GetShowTestingBrowserHost(serverTestRunConfiguration, serverTestRunConfiguration.ShowTestingBrowserHost);

            var querystring = "?{0}={1}".FormatWith(StatLightServiceRestApi.StatLightResultPostbackUrl,
                                                   HttpUtility.UrlEncode(location.BaseUrl.ToString()));
            var testPageUrlAndPostbackQuerystring = new Uri(location.TestPageUrl + querystring);
            logger.Debug("testPageUrlAndPostbackQuerystring={0}".FormatWith(testPageUrlAndPostbackQuerystring.ToString()));
            var webBrowsers = GetWebBrowsers(logger, testPageUrlAndPostbackQuerystring, clientTestRunConfiguration, showTestingBrowserHost, serverTestRunConfiguration.QueryString, statLightConfiguration.Server.ForceBrowserStart);

            var dialogMonitorRunner = SetupDialogMonitorRunner(logger, webBrowsers, debugAssertMonitorTimer);

            StartupBrowserCommunicationTimeoutMonitor(new TimeSpan(0, 0, 5, 0));
            CreateAndAddConsoleResultHandlerToEventAggregator(logger);

            IRunner runner = new OnetimeRunner(logger, _eventAggregator, webServer, webBrowsers, statLightConfiguration.Server.XapToTestPath, dialogMonitorRunner);
            return runner;
        }

        private IDialogMonitorRunner SetupDialogMonitorRunner(ILogger logger, List<IWebBrowser> webBrowsers, TimerWrapper debugAssertMonitorTimer)
        {
            var dialogMonitors = new List<IDialogMonitor>
                                     {
                                         new DebugAssertMonitor(logger),
                                     };

            foreach (var webBrowser in webBrowsers)
            {
                var monitor = new MessageBoxMonitor(logger, webBrowser);
                dialogMonitors.Add(monitor);
            }

            return new DialogMonitorRunner(logger, _eventAggregator, debugAssertMonitorTimer, dialogMonitors);
        }

        private static bool GetShowTestingBrowserHost(ServerTestRunConfiguration serverTestRunConfiguration, bool showTestingBrowserHost)
        {
            // The new March/April 2010 will fail in the "minimized mode" 
            //TODO figure out how to not get the errors when these are minimized
            if (serverTestRunConfiguration.XapHostType == XapHostType.MSTestMarch2010 ||
                serverTestRunConfiguration.XapHostType == XapHostType.MSTestApril2010)
                showTestingBrowserHost = true;
            return showTestingBrowserHost;
        }
    }
}
