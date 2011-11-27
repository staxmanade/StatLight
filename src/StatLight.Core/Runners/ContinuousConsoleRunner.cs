


namespace StatLight.Core.Runners
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events;
    using StatLight.Core.Monitoring;
    using StatLight.Core.Properties;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;

    internal class ContinuousConsoleRunner : IRunner,
        IListener<XapFileBuildChangedServerEvent>
    {
        private readonly object _sync = new object();
        private readonly ILogger _logger;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;
        private readonly IEventPublisher _eventPublisher;
        private readonly IWebServer _webServer;
        private readonly IEnumerable<IWebBrowser> _webBrowsers;
        private readonly ResponseFactory _responseFactory;
        private readonly XapFileBuildChangedMonitor _xapFileBuildChangedMonitor;
        private readonly Queue<XapFileBuildChangedServerEvent> _queuedRuns = new Queue<XapFileBuildChangedServerEvent>();
        private readonly Dictionary<string, StatLightConfiguration> _statLightConfigurations;
        private readonly IDialogMonitorRunner _dialogMonitorRunner;
        private readonly Thread _runnerTask;
        private readonly List<XapFileBuildChangedServerEvent> _initialXaps = new List<XapFileBuildChangedServerEvent>();
        private string _currentFilterString;
        private bool _isRunning = false;

        internal ContinuousConsoleRunner(
            ILogger logger,
            IEventSubscriptionManager eventSubscriptionManager,
            IEventPublisher eventPublisher,
            IEnumerable<StatLightConfiguration> statLightConfigurations,
            IWebServer webServer,
            IEnumerable<IWebBrowser> webBrowsers,
            ResponseFactory responseFactory,
            IDialogMonitorRunner dialogMonitorRunner)
        {
            if (statLightConfigurations == null) throw new ArgumentNullException("statLightConfigurations");

            eventSubscriptionManager.AddListener(this);
            _logger = logger;
            _eventSubscriptionManager = eventSubscriptionManager;
            _eventPublisher = eventPublisher;
            _webServer = webServer;
            _webBrowsers = webBrowsers;
            _responseFactory = responseFactory;
            _dialogMonitorRunner = dialogMonitorRunner;
            Func<string, string> getFullXapPath = path => new FileInfo(path).FullName;
            var xapFiles = statLightConfigurations.Select(x => getFullXapPath(x.Server.XapToTestPath));
            _statLightConfigurations = statLightConfigurations.ToDictionary(x => getFullXapPath(x.Server.XapToTestPath), x => x);
            _xapFileBuildChangedMonitor = new XapFileBuildChangedMonitor(eventPublisher, xapFiles);
            xapFiles.Each(e => _initialXaps.Add(new XapFileBuildChangedServerEvent(e)));
            _currentFilterString = statLightConfigurations.First().Client.TagFilter;
            QueueInitialXaps();

            _runnerTask = new Thread(() =>
            {
                TryRun();

                while (!ShouldExitFromInput())
                {
                    TryRun();
                }
            });
        }

        private void QueueInitialXaps()
        {
            foreach (var item in _initialXaps)
            {
                _queuedRuns.Enqueue(item);
            }
        }

        private bool ShouldExitFromInput()
        {
            "*** Type [?] for more info***".WrapConsoleMessageWithColor(Settings.Default.ConsoleColorWarning, true);
            "Current Filter <{0}>: ".FormatWith(_currentFilterString).WrapConsoleMessageWithColor(Settings.Default.ConsoleColorWarning, false);
            var input = Console.ReadLine() ?? string.Empty;

            if (input.Equals("exit", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (input.Equals("?", StringComparison.OrdinalIgnoreCase))
            {
                PrintHelp();
                return ShouldExitFromInput();
            }

            if (input.Equals("cf", StringComparison.OrdinalIgnoreCase) ||
                input.Equals("clearfilter", StringComparison.OrdinalIgnoreCase))
            {
                input = string.Empty;
            }
            else if (string.IsNullOrEmpty(input))
            {
                input = _currentFilterString;
            }

            _currentFilterString = input;

            QueueInitialXaps();

            return false;
        }

        private static void PrintHelp()
        {
            Action<string> w = msg => "* {0}"
                .FormatWith(msg)
                .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);

            w("************************ StatLight Continuous Mode Help **************************");
            w(" ");
            w(" Commands:");
            w("   - exit - Quits stops running the continuous mode and exits the application.");
            w("   - [cf|clearfilter] - Clears any set filter.");
            w("   - {Any Other Input} - The input is passed to the client for 'TagFilter'.");
            w("                         This allows you to specify a filter to narrow down");
            w("                         a set of tests to run.");
            w("");
            w("**********************************************************************************");
        }

        public TestReport Run()
        {
            _runnerTask.Start();
            _runnerTask.Join();

            return new TestReport(string.Empty);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "line")]
        private void TryRun()
        {
            _isRunning = true;
            var testReportCollection = new TestReportCollection();
            DateTime startOfRunTime = DateTime.Now;

            while (_queuedRuns.Count > 0)
            {
                XapFileBuildChangedServerEvent buildEvent;

                lock (_sync)
                {
                    buildEvent = _queuedRuns.Dequeue();
                }

                StatLightConfiguration statLightConfiguration = _statLightConfigurations[buildEvent.XapPath];
                statLightConfiguration.Client.TagFilter = _currentFilterString;

                _responseFactory.ReplaceCurrentItems(statLightConfiguration.Server.HostXap, statLightConfiguration.Client);

                using (var onetimeRunner = new OnetimeRunner(
                    logger: _logger,
                    eventSubscriptionManager: _eventSubscriptionManager,
                    eventPublisher: _eventPublisher,
                    webServer: _webServer,
                    webBrowsers: _webBrowsers,
                    xapPath: buildEvent.XapPath,
                    dialogMonitorRunner: _dialogMonitorRunner))
                {
                    TestReport testReport = onetimeRunner.Run();
                    testReportCollection.Add(testReport);
                }
            }

            _isRunning = false;

            if (testReportCollection.Any())
                ConsoleTestCompleteMessage.PrintFinalTestSummary(testReportCollection, startOfRunTime);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _xapFileBuildChangedMonitor.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Handle(XapFileBuildChangedServerEvent message)
        {
            lock (_sync)
            {
                _queuedRuns.Enqueue(message);
            }

            if (!_isRunning)
                TryRun();
        }
    }
}
