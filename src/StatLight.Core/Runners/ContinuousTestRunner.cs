
using StatLight.Core.Configuration;
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
        private readonly IWebBrowser _webBrowser;
        private readonly ClientTestRunConfiguration _clientTestRunConfiguration;
        private readonly IXapFileBuildChangedMonitor _xapFileBuildChangedMonitor;
        private readonly ILogger _logger;
        private TestResultAggregator _testResultAggregator;
        private readonly IEventAggregator _eventAggregator;
        private DateTime _startOfRun;
        private string _xapPath;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ContinuousTestRunner"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ctor")]
        internal ContinuousTestRunner(
            ILogger logger,
            IEventAggregator eventAggregator,
            IWebBrowser webBrowser,
            ClientTestRunConfiguration clientTestRunConfiguration,
            IXapFileBuildChangedMonitor xapFileBuildChangedMonitor,
            string xapPath)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _webBrowser = webBrowser;
            _clientTestRunConfiguration = clientTestRunConfiguration;
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
            _webBrowser.Start();
            IsCurrentlyRunningTest = true;
        }

        private void Stop()
        {
            _logger.Debug("ContinuousTestRunner.Stop()");
            ConsoleTestCompleteMessage.WriteOutCompletionStatement(_testResultAggregator.CurrentReport, _startOfRun);
            _webBrowser.Stop();
            IsCurrentlyRunningTest = false;

            _eventAggregator.RemoveListener(_testResultAggregator);
            _testResultAggregator.Dispose();
            _testResultAggregator = null;
        }

        public void ForceFilteredTest(string newTagFilter)
        {
            _logger.Debug("ContinuousTestRunner.ForceTest(tempTagFilter=[{0}])".FormatWith(newTagFilter));
            _clientTestRunConfiguration.TagFilter = newTagFilter;
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
