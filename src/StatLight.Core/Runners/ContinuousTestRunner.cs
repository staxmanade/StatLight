

namespace StatLight.Core.Runners
{
    using System;
    using System.Threading;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;
    using StatLight.Core.Events;
    using EventAggregatorNet;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;

    internal class ContinuousTestRunner : IDisposable,
        IListener<TestRunCompletedServerEvent>,
        IListener<XapFileBuildChangedServerEvent>
    {
        private readonly IWebBrowser _webBrowser;
        private readonly ClientTestRunConfiguration _clientTestRunConfiguration;
        private readonly ILogger _logger;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;
        private TestResultAggregator _testResultAggregator;
        private DateTime _startOfRun;
        private string _xapPath;
        private IEventPublisher _eventPublisher;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ContinuousTestRunner"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ctor")]
        internal ContinuousTestRunner(
            ILogger logger,
            IEventSubscriptionManager eventSubscriptionManager,
            IEventPublisher eventPublisher,
            IWebBrowser webBrowser,
            ClientTestRunConfiguration clientTestRunConfiguration,
            string xapPath)
        {
            _logger = logger;
            _eventSubscriptionManager = eventSubscriptionManager;
            _eventPublisher = eventPublisher;
            _webBrowser = webBrowser;
            _clientTestRunConfiguration = clientTestRunConfiguration;
            _xapPath = xapPath;

            _eventSubscriptionManager.AddListener(this);

            _logger.Debug("ContinuousTestRunner.ctor()");

            Start();
        }

        public bool IsCurrentlyRunningTest { get; private set; }

        private void Start()
        {
            _testResultAggregator = new TestResultAggregator(_logger, _eventPublisher, _xapPath);
            _eventSubscriptionManager.AddListener(_testResultAggregator);

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

            _eventSubscriptionManager.RemoveListener(_testResultAggregator);
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
                _eventSubscriptionManager.RemoveListener(this);
                if (_testResultAggregator != null)
                    _testResultAggregator.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Handle(TestRunCompletedServerEvent message)
        {
            Stop();
        }

        public void Handle(XapFileBuildChangedServerEvent message)
        {
            _logger.Debug("Xap file changed detected.");
            if (!IsCurrentlyRunningTest)
            {
                // let the file system finish flushing anything out before we start up a new test run
                Thread.Sleep(2000);
                Start();
            }
        }
    }
}
