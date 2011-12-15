


using System.Diagnostics;

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
        private readonly XapFileBuildChangedMonitor _xapFileBuildChangedMonitor;
        private readonly Queue<XapFileBuildChangedServerEvent> _queuedRuns = new Queue<XapFileBuildChangedServerEvent>();
        private readonly Dictionary<string, StatLightConfiguration> _statLightConfigurations;
        private readonly IDialogMonitorRunner _dialogMonitorRunner;
        private readonly ICurrentStatLightConfiguration _currentStatLightConfiguration;
        private readonly Thread _runnerTask;
        private readonly List<XapFileBuildChangedServerEvent> _initialXaps = new List<XapFileBuildChangedServerEvent>();
        private string _currentFilterString;
        private bool _isRunning = false;

        internal ContinuousConsoleRunner(
            ILogger logger,
            IEventSubscriptionManager eventSubscriptionManager,
            IEventPublisher eventPublisher,
            IWebServer webServer,
            IEnumerable<IWebBrowser> webBrowsers,
            IDialogMonitorRunner dialogMonitorRunner,
            ICurrentStatLightConfiguration currentStatLightConfiguration)
        {
            eventSubscriptionManager.AddListener(this);
            _logger = logger;
            _eventSubscriptionManager = eventSubscriptionManager;
            _eventPublisher = eventPublisher;
            _webServer = webServer;
            _webBrowsers = webBrowsers;
            _dialogMonitorRunner = dialogMonitorRunner;
            _currentStatLightConfiguration = currentStatLightConfiguration;
            Func<string, string> getFullXapPath = path => new FileInfo(path).FullName;
            var xapFiles = currentStatLightConfiguration.Select(x => getFullXapPath(x.Server.XapToTestPath));
            _statLightConfigurations = currentStatLightConfiguration.ToDictionary(x => getFullXapPath(x.Server.XapToTestPath), x => x);
            _xapFileBuildChangedMonitor = new XapFileBuildChangedMonitor(eventPublisher, xapFiles);
            xapFiles.Each(e => _initialXaps.Add(new XapFileBuildChangedServerEvent(e)));
            _currentFilterString = currentStatLightConfiguration.First().Client.TagFilter;
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
            Stopwatch multiRunStopwatch = Stopwatch.StartNew();

            while (_queuedRuns.Count > 0)
            {
                XapFileBuildChangedServerEvent buildEvent;

                lock (_sync)
                {
                    buildEvent = _queuedRuns.Dequeue();
                }

                var xapPath = buildEvent.XapPath;

                _currentStatLightConfiguration.SetCurrentTo(xapPath);

                _currentStatLightConfiguration.Current.Client.TagFilter = _currentFilterString;

                using (var onetimeRunner = new OnetimeRunner(
                    logger: _logger,
                    eventSubscriptionManager: _eventSubscriptionManager,
                    eventPublisher: _eventPublisher,
                    webServer: _webServer,
                    webBrowsers: _webBrowsers,
                    xapPath: xapPath,
                    dialogMonitorRunner: _dialogMonitorRunner))
                {
                    Stopwatch singleRunStopwatch = Stopwatch.StartNew();
                    TestReport testReport = onetimeRunner.Run();
                    singleRunStopwatch.Stop();
                    testReportCollection.Add(testReport);
                    _eventPublisher.SendMessage(new TestReportGeneratedServerEvent(testReport, singleRunStopwatch.Elapsed, shouldPrintSummary: _statLightConfigurations.Count > 1));
                }
            }

            multiRunStopwatch.Stop();
            _isRunning = false;

            if (testReportCollection.Any())
            {
                _eventPublisher.SendMessage(new TestReportCollectionGeneratedServerEvent(testReportCollection, multiRunStopwatch.Elapsed));
            }
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
            if (message == null) throw new ArgumentNullException("message");

            _logger.Debug("ContinuousConsoleRunner.Handle<XapFileBuildChangedServerEvent> - {0} - {1}".FormatWith(DateTime.Now, message.XapPath));

            lock (_sync)
            {
                _queuedRuns.Enqueue(message);
            }

            if (!_isRunning)
                TryRun();
        }
    }
}
