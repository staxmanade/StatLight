
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Runners
{
    using System;
    using System.Threading;
    using StatLight.Core.Common;
    using StatLight.Core.Reporting;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;
    using StatLight.Core.Events;
    using StatLight.Core.Reporting.Providers.Console;

    internal class ContinuousTestRunner : IDisposable
    {
        private readonly IBrowserFormHost _browserFormHost;
        private readonly IStatLightService _statLightService;
        private readonly IXapFileBuildChangedMonitor _xapFileBuildChangedMonitor;
        private readonly ILogger _logger;
        private TestResultAggregator _testResultAggregator;
        private readonly IEventAggregator _eventAggregator;
        private DateTime _startOfRun;
        private string _xapPath;

        internal ContinuousTestRunner(
            ILogger logger,
            IEventAggregator eventAggregator,
            IBrowserFormHost browserFormHost,
            IStatLightService statLightService,
            IXapFileBuildChangedMonitor xapFileBuildChangedMonitor,
            string xapPath)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _browserFormHost = browserFormHost;
            _statLightService = statLightService;
            _xapFileBuildChangedMonitor = xapFileBuildChangedMonitor;
            _xapPath = xapPath;

            _logger.Debug("ContinuousTestRunner.ctor()");

            _xapFileBuildChangedMonitor.FileChanged += (sender, e) =>
            {
                _logger.Debug("Xap file changed detected.");
                if (!IsCurrentlyRunningTest)
                {
                    // let the file system finish flushing anything out before we start up a new test run
                    Thread.Sleep(2000);
                    Start();
                }
            };

            eventAggregator
                .AddListener<TestRunCompletedServerEvent>(e => Stop());

            Start();
        }

        public bool IsCurrentlyRunningTest { get; private set; }

        private void Start()
        {
            _testResultAggregator = new TestResultAggregator(_logger, _eventAggregator, _xapPath);
            _eventAggregator.AddListener(_testResultAggregator);

            _logger.Information("{1}{1}Starting Test Run: {0}{1}{1}"
                .FormatWith(DateTime.Now, Environment.NewLine));
            _startOfRun = DateTime.Now;
            _browserFormHost.Start();
            IsCurrentlyRunningTest = true;
        }

        private void Stop()
        {
            _logger.Debug("ContinuousTestRunner.Stop()");
            ConsoleTestCompleteMessage.WriteOutCompletionStatement(_testResultAggregator.CurrentReport, _startOfRun);
            _browserFormHost.Stop();
            IsCurrentlyRunningTest = false;

            _eventAggregator.RemoveListener(_testResultAggregator);
            _testResultAggregator.Dispose();
            _testResultAggregator = null;
        }

        public void ForceFilteredTest(string newTagFilter)
        {
            _logger.Debug("ContinuousTestRunner.ForceTest(tempTagFilter=[{0}])".FormatWith(newTagFilter));
            _statLightService.TagFilters = newTagFilter;
            Start();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_testResultAggregator != null)
                    _testResultAggregator.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
