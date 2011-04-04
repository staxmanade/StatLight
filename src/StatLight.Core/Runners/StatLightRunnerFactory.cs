


namespace StatLight.Core.Runners
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Common;
    using StatLight.Core.Common.Abstractions.Timing;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Monitoring;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.Reporting.Providers.TeamCity;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;

    public class StatLightRunnerFactory : IStatLightRunnerFactory
    {
        private readonly ILogger _logger;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;
        private readonly IEventPublisher _eventPublisher;
        private BrowserCommunicationTimeoutMonitor _browserCommunicationTimeoutMonitor;
        private ConsoleResultHandler _consoleResultHandler;

        public StatLightRunnerFactory(ILogger logger)
            : this(logger, new EventAggregator(logger))
        {
        }

        internal StatLightRunnerFactory(ILogger logger, EventAggregator eventAggregator) : this(logger, eventAggregator, eventAggregator) { }

        public StatLightRunnerFactory(ILogger logger, IEventSubscriptionManager eventSubscriptionManager, IEventPublisher eventPublisher)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
            _eventSubscriptionManager = eventSubscriptionManager;
            _eventPublisher = eventPublisher;

            var debugListener = new ConsoleDebugListener(logger);
            _eventSubscriptionManager.AddListener(debugListener);

            var ea = eventSubscriptionManager as EventAggregator;
            if (ea != null)
            {
                ea.IgnoreTracingEvent<InitializationOfUnitTestHarnessClientEvent>();
                ea.IgnoreTracingEvent<TestExecutionClassCompletedClientEvent>();
                ea.IgnoreTracingEvent<TestExecutionClassBeginClientEvent>();
                ea.IgnoreTracingEvent<SignalTestCompleteClientEvent>();
            }

            SetupExtensions(_eventSubscriptionManager);
        }


        private static string GetFullPath(string path)
        {
            if (!Path.IsPathRooted(path) && AppDomain.CurrentDomain.BaseDirectory != null)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            return Path.GetFullPath(path);
        }


        private void SetupExtensions(IEventSubscriptionManager eventSubscriptionManager)
        {
            try
            {
                var path = GetFullPath("Extensions");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                using (var directoryCatalog = new DirectoryCatalog(path))
                using (var compositionContainer = new CompositionContainer(directoryCatalog))
                {
                    
                    var extensions = compositionContainer.GetExports<ITestingReportEvents>().ToList();
                    if (extensions.Any())
                    {
                        _logger.Debug("********** Extensions **********");
                        foreach (var lazyExtension in extensions)
                        {
                            var extensionInstance = lazyExtension.Value;
                            _logger.Debug("* Adding - {0}".FormatWith(extensionInstance.GetType().FullName));
                            eventSubscriptionManager.AddListener(extensionInstance);
                        }
                        _logger.Debug("********************************");
                    }
                }
            }
            catch (ReflectionTypeLoadException rfex)
            {
                string loaderExceptionMessages = "";
                foreach (var t in rfex.LoaderExceptions)
                {
                    loaderExceptionMessages += "   -  ";
                    loaderExceptionMessages += t.Message;
                    loaderExceptionMessages += Environment.NewLine;
                }

                string msg = @"
********************* ReflectionTypeLoadException *********************
***** Begin Loader Exception Messages *****
{0}
***** End Loader Exception Messages *****
".FormatWith(loaderExceptionMessages);

                _logger.Error(msg);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to initialize extension. Error:{0}{1}".FormatWith(Environment.NewLine, e.ToString()));
            }
        }

        public IRunner CreateContinuousTestRunner(StatLightConfiguration statLightConfiguration)
        {
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            IWebServer webServer;
            List<IWebBrowser> webBrowsers;
            IDialogMonitorRunner dialogMonitorRunner;

            BuildAndReturnWebServiceAndBrowser(
                _logger,
                statLightConfiguration.Server.ShowTestingBrowserHost,
                statLightConfiguration,
                out webServer,
                out webBrowsers,
                out dialogMonitorRunner);

            CreateAndAddConsoleResultHandlerToEventAggregator(_logger);

            IRunner runner = new ContinuousConsoleRunner(_logger, _eventSubscriptionManager, _eventPublisher, statLightConfiguration.Server.XapToTestPath, statLightConfiguration.Client, webServer, webBrowsers.First());
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
                statLightConfiguration.Server.ShowTestingBrowserHost,
                statLightConfiguration,
                out webServer,
                out webBrowsers,
                out dialogMonitorRunner);

            CreateAndAddConsoleResultHandlerToEventAggregator(_logger);
            IRunner runner = new OnetimeRunner(_logger, _eventSubscriptionManager, _eventPublisher, webServer, webBrowsers, statLightConfiguration.Server.XapToTestPath, dialogMonitorRunner);
            return runner;
        }

        public IRunner CreateWebServerOnlyRunner(StatLightConfiguration statLightConfiguration)
        {
            if (statLightConfiguration == null) throw new ArgumentNullException("statLightConfiguration");
            var location = new WebServerLocation(_logger);

            var webServer = CreateWebServer(_logger, statLightConfiguration, location);
            CreateAndAddConsoleResultHandlerToEventAggregator(_logger);
            IRunner runner = new WebServerOnlyRunner(_logger, _eventSubscriptionManager, _eventPublisher, webServer, location.TestPageUrl, statLightConfiguration.Server.XapToTestPath);

            return runner;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal IWebServer CreateWebServer(ILogger logger, StatLightConfiguration statLightConfiguration, WebServerLocation webServerLocation)
        {
            var responseFactory = new ResponseFactory(statLightConfiguration.Server.HostXap, statLightConfiguration.Client);
            var postHandler = new PostHandler(logger, _eventPublisher, statLightConfiguration.Client, responseFactory);

            return new InMemoryWebServer(logger, webServerLocation, responseFactory, postHandler);
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
            webServer = CreateWebServer(logger, statLightConfiguration, location);

            webBrowsers = GetWebBrowsers(logger, location.TestPageUrl, clientTestRunConfiguration, showTestingBrowserHost, serverTestRunConfiguration.QueryString, statLightConfiguration.Server.ForceBrowserStart);

            dialogMonitorRunner = SetupDialogMonitorRunner(logger, webBrowsers, debugAssertMonitorTimer);

            StartupBrowserCommunicationTimeoutMonitor();
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
                .ToList();
            return webBrowsers;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private void StartupBrowserCommunicationTimeoutMonitor()
        {
            if (_browserCommunicationTimeoutMonitor == null)
            {
                _browserCommunicationTimeoutMonitor = new BrowserCommunicationTimeoutMonitor(_eventPublisher, new TimerWrapper(3000), TimeSpan.FromMinutes(5));
                _eventSubscriptionManager.AddListener(_browserCommunicationTimeoutMonitor);
            }
        }

        private void CreateAndAddConsoleResultHandlerToEventAggregator(ILogger logger)
        {
            if (_consoleResultHandler == null)
            {
                _consoleResultHandler = new ConsoleResultHandler(logger);
                _eventSubscriptionManager.AddListener(_consoleResultHandler);
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

            return new DialogMonitorRunner(logger, _eventPublisher, debugAssertMonitorTimer, dialogMonitors);
        }
    }
}
