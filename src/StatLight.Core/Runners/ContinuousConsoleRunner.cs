


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

            xapFiles.Each(e => _queuedRuns.Enqueue(new XapFileBuildChangedServerEvent(e)));


            _runnerTask = new Thread(() =>
            {
                TryRun();

                string line;
                while (!(line = System.Console.ReadLine()).Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    TryRun(line);
                    //runner.ForceFilteredTest(line);
                }
            });
        }

        public TestReport Run()
        {
            _runnerTask.Start();
            _runnerTask.Join();

            return new TestReport(string.Empty);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "line")]
        private void TryRun(string line = null)
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

                _logger.Debug("buildEvent:{0}".FormatWith(buildEvent.XapPath));
                _statLightConfigurations.Each(e => _logger.Debug("loaded Config:{0}".FormatWith(e.Key)));
                StatLightConfiguration statLightConfiguration = _statLightConfigurations[buildEvent.XapPath];

                _responseFactory.ReplaceCurrentItems(statLightConfiguration.Server.HostXap, statLightConfiguration.Client);

                using (var onetimeRunner = new OnetimeRunner(_logger, _eventSubscriptionManager, _eventPublisher, _webServer, _webBrowsers,
                                                      buildEvent.XapPath, _dialogMonitorRunner))
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
