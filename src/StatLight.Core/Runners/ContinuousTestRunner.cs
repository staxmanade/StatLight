
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
        private readonly ITestResultHandler _testResultHandler;
        private readonly IEventAggregator _eventAggregator;

        internal ContinuousTestRunner(
            ILogger logger,
            IEventAggregator eventAggregator,
            IBrowserFormHost browserFormHost,
            IStatLightService statLightService,
            IXapFileBuildChangedMonitor xapFileBuildChangedMonitor,
            ITestResultHandler testResultHandler)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _browserFormHost = browserFormHost;
            _statLightService = statLightService;
            _xapFileBuildChangedMonitor = xapFileBuildChangedMonitor;
            _testResultHandler = testResultHandler;

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
                .AddListener<TestRunCompletedEvent>(e => Stop());

            Start();
        }

        public bool IsCurrentlyRunningTest { get; private set; }

        private void Start()
        {
            _testResultAggregator = new TestResultAggregator(_testResultHandler, _eventAggregator);

            _logger.Information("{1}{1}Starting Test Run: {0}{1}{1}"
                .FormatWith(DateTime.Now, Environment.NewLine));
            _browserFormHost.Start();
            IsCurrentlyRunningTest = true;
        }

        private void Stop()
        {
            _logger.Debug("ContinuousTestRunner.Stop()");
            _logger.Information("{1}{1}--- Completed Test Run: {0}{1}{1}"
                .FormatWith(DateTime.Now, Environment.NewLine));
            ConsoleTestCompleteMessage.WriteOutCompletionStatement(_testResultAggregator.CurrentReport);
            _browserFormHost.Stop();
            IsCurrentlyRunningTest = false;
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
