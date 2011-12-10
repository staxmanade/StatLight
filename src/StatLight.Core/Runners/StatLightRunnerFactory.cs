namespace StatLight.Core.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using StatLight.Core.Common;
    using StatLight.Core.Common.Abstractions.Timing;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events;
    using StatLight.Core.Monitoring;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.Reporting.Providers.TeamCity;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;
    using TinyIoC;

    public class StatLightRunnerFactory : IStatLightRunnerFactory
    {
        private readonly TinyIoCContainer _ioc;
        private readonly ILogger _logger;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;
        private readonly IEventPublisher _eventPublisher;
        private BrowserCommunicationTimeoutMonitor _browserCommunicationTimeoutMonitor;
        private bool _hasConsoleResultHandlerBeenAddeToEventAgregator;

        public StatLightRunnerFactory(ILogger logger, TinyIoCContainer ioc)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (ioc == null) throw new ArgumentNullException("ioc");

            _ioc = ioc;
            _logger = logger;

            _eventSubscriptionManager = ioc.Resolve<IEventSubscriptionManager>();
            _eventPublisher = ioc.Resolve<IEventPublisher>();

            _ioc.ResolveAndAddToEventAggregator<ConsoleDebugListener>();

            ioc.Resolve<ExtensionResolver>().AddExtensionsToEventAggregator();
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IRunner CreateContinuousTestRunner(IEnumerable<StatLightConfiguration> statLightConfigurations)
        {
            if (statLightConfigurations == null)
                throw new ArgumentNullException("statLightConfigurations");

            var firstStatLightConfig = statLightConfigurations.First();

            var webServer = CreateWebServer(_logger, firstStatLightConfig);
            var responseFactory = _ioc.Resolve<ResponseFactory>();

            List<IWebBrowser> webBrowsers = GetWebBrowsers(
                logger: _logger,
                webBrowserType: firstStatLightConfig.Client.WebBrowserType,
                queryString: firstStatLightConfig.Server.QueryString,
                forceBrowserStart: firstStatLightConfig.Server.ForceBrowserStart,
                windowGeometry: firstStatLightConfig.Client.WindowGeometry,
                numberOfBrowserHosts: firstStatLightConfig.Client.NumberOfBrowserHosts);

            IDialogMonitorRunner dialogMonitorRunner = SetupDialogMonitorRunner(_logger, webBrowsers);
            
            StartupBrowserCommunicationTimeoutMonitor();

            CreateAndAddConsoleResultHandlerToEventAggregator();

            return new ContinuousConsoleRunner(_logger, _eventSubscriptionManager, _eventPublisher, statLightConfigurations,
                                        webServer, webBrowsers, responseFactory, dialogMonitorRunner);
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
                statLightConfiguration,
                out webServer,
                out webBrowsers,
                out dialogMonitorRunner);

            var teamCityTestResultHandler = new TeamCityTestResultHandler(new ConsoleCommandWriter(), statLightConfiguration.Server.XapToTestPath);
            IRunner runner = new TeamCityRunner(logger, _eventSubscriptionManager, _eventPublisher, webServer, webBrowsers, teamCityTestResultHandler, statLightConfiguration.Server.XapToTestPath, dialogMonitorRunner);

            return runner;
        }

        public IRunner CreateOnetimeConsoleRunner(StatLightConfiguration statLightConfiguration)
        {
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            IWebServer webServer;
            List<IWebBrowser> webBrowsers;
            IDialogMonitorRunner dialogMonitorRunner;

            BuildAndReturnWebServiceAndBrowser(
                _logger,
                statLightConfiguration,
                out webServer,
                out webBrowsers,
                out dialogMonitorRunner);

            CreateAndAddConsoleResultHandlerToEventAggregator();
            IRunner runner = new OnetimeRunner(_logger, _eventSubscriptionManager, _eventPublisher, webServer, webBrowsers, statLightConfiguration.Server.XapToTestPath, dialogMonitorRunner);
            return runner;
        }

        public IRunner CreateWebServerOnlyRunner(StatLightConfiguration statLightConfiguration)
        {
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            var location = new WebServerLocation(_logger);

            var webServer = CreateWebServer(_logger, statLightConfiguration);
            CreateAndAddConsoleResultHandlerToEventAggregator();
            IRunner runner = new WebServerOnlyRunner(_logger, _eventSubscriptionManager, _eventPublisher, webServer, location.TestPageUrl, statLightConfiguration.Server.XapToTestPath);

            return runner;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private IWebServer CreateWebServer(ILogger logger, StatLightConfiguration statLightConfiguration)
        {
            var responseFactory = new ResponseFactory(statLightConfiguration.Server.HostXap, statLightConfiguration.Client);
            var postHandler = new PostHandler(logger, _eventPublisher, statLightConfiguration.Client, responseFactory);

            _ioc.Register(responseFactory);
            _ioc.Register<IPostHandler>(postHandler);

            return _ioc.Resolve<InMemoryWebServer>();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void BuildAndReturnWebServiceAndBrowser(
            ILogger logger,
            StatLightConfiguration statLightConfiguration,
            out IWebServer webServer,
            out List<IWebBrowser> webBrowsers,
            out IDialogMonitorRunner dialogMonitorRunner)
        {
            ClientTestRunConfiguration clientTestRunConfiguration = statLightConfiguration.Client;
            ServerTestRunConfiguration serverTestRunConfiguration = statLightConfiguration.Server;

            webServer = CreateWebServer(logger, statLightConfiguration);

            webBrowsers = GetWebBrowsers(
                logger: logger,
                webBrowserType: clientTestRunConfiguration.WebBrowserType,
                queryString: serverTestRunConfiguration.QueryString,
                forceBrowserStart: serverTestRunConfiguration.ForceBrowserStart,
                windowGeometry: clientTestRunConfiguration.WindowGeometry,
                numberOfBrowserHosts: clientTestRunConfiguration.NumberOfBrowserHosts);

            dialogMonitorRunner = SetupDialogMonitorRunner(logger, webBrowsers);

            StartupBrowserCommunicationTimeoutMonitor();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "testPageUrlWithQueryString")]
        private List<IWebBrowser> GetWebBrowsers(ILogger logger, WebBrowserType webBrowserType, string queryString, bool forceBrowserStart, WindowGeometry windowGeometry, int numberOfBrowserHosts)
        {
            var webServerLocation = _ioc.Resolve<WebServerLocation>();
            Uri testPageUrl = webServerLocation.TestPageUrl;

            var webBrowserFactory = _ioc.Resolve<WebBrowserFactory>();
            var testPageUrlWithQueryString = new Uri(testPageUrl + "?" + queryString);
            logger.Debug("testPageUrlWithQueryString = " + testPageUrlWithQueryString);
            List<IWebBrowser> webBrowsers = Enumerable
                .Range(1, numberOfBrowserHosts)
                .Select(browserI => webBrowserFactory.Create(webBrowserType, testPageUrlWithQueryString, forceBrowserStart, numberOfBrowserHosts > 1, windowGeometry))
                .ToList();
            return webBrowsers;
        }

        private void StartupBrowserCommunicationTimeoutMonitor()
        {
            if (_browserCommunicationTimeoutMonitor == null)
            {
                _browserCommunicationTimeoutMonitor = new BrowserCommunicationTimeoutMonitor(_eventPublisher, new TimerWrapper(3000), TimeSpan.FromMinutes(5));
                _eventSubscriptionManager.AddListener(_browserCommunicationTimeoutMonitor);
            }
        }

        private void CreateAndAddConsoleResultHandlerToEventAggregator()
        {
            if (_hasConsoleResultHandlerBeenAddeToEventAgregator == false)
            {
                _hasConsoleResultHandlerBeenAddeToEventAgregator = true;

                _ioc.ResolveAndAddToEventAggregator<ConsoleResultHandler>();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public IRunner CreateRemotelyHostedRunner(StatLightConfiguration statLightConfiguration)
        {
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");

            ClientTestRunConfiguration clientTestRunConfiguration = statLightConfiguration.Client;
            ServerTestRunConfiguration serverTestRunConfiguration = statLightConfiguration.Server;

            throw new NotImplementedException();
            //var urlToTestPage = statLightConfiguration.Client.XapToTestUrl.ToUri();

            //var location = new RemoteSiteOverriddenLocation(logger, urlToTestPage);
            //var debugAssertMonitorTimer = new TimerWrapper(serverTestRunConfiguration.DialogSmackDownElapseMilliseconds);
            //SetupDebugClientEventListener(logger);
            //var webServer = CreateWebServer(logger, statLightConfiguration, location);
            //
            //var showTestingBrowserHost = serverTestRunConfiguration.ShowTestingBrowserHost;
            //
            //var querystring = "?{0}={1}".FormatWith(StatLightServiceRestApi.StatLightResultPostbackUrl,
            //                                       HttpUtility.UrlEncode(location.BaseUrl.ToString()));
            //var testPageUrlAndPostbackQuerystring = new Uri(location.TestPageUrl + querystring);
            //logger.Debug("testPageUrlAndPostbackQuerystring={0}".FormatWith(testPageUrlAndPostbackQuerystring.ToString()));
            //var webBrowsers = GetWebBrowsers(logger, testPageUrlAndPostbackQuerystring, clientTestRunConfiguration, showTestingBrowserHost, serverTestRunConfiguration.QueryString, statLightConfiguration.Server.ForceBrowserStart);
            //
            //var dialogMonitorRunner = SetupDialogMonitorRunner(logger, webBrowsers, debugAssertMonitorTimer);
            //
            //StartupBrowserCommunicationTimeoutMonitor();
            //CreateAndAddConsoleResultHandlerToEventAggregator(logger);
            //
            //IRunner runner = new OnetimeRunner(logger, _eventSubscriptionManager, _eventPublisher, webServer, webBrowsers, statLightConfiguration.Server.XapToTestPath, dialogMonitorRunner);
            //return runner;
        }

        private IDialogMonitorRunner SetupDialogMonitorRunner(ILogger logger, IEnumerable<IWebBrowser> webBrowsers)
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

            return new DialogMonitorRunner(logger, _eventPublisher, dialogMonitors);
        }
    }
}
