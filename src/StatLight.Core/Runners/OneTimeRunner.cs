
using System.Linq;

namespace StatLight.Core.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using StatLight.Core.Common;
    using StatLight.Core.Events;
    using StatLight.Core.Events;
    using StatLight.Core.Monitoring;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;

    internal class OnetimeRunner : IRunner,
        IListener<TestRunCompletedServerEvent>
    {
        private readonly ILogger _logger;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;
        private readonly IWebServer _webServer;
        private readonly List<IWebBrowser> _webBrowsers;
        private readonly string _xapPath;
        private readonly IDialogMonitorRunner _dialogMonitorRunner;
        private readonly TestResultAggregator _testResultAggregator;
        readonly AutoResetEvent _browserThreadWaitHandle = new AutoResetEvent(false);

        internal OnetimeRunner(
            ILogger logger,
            IEventSubscriptionManager eventSubscriptionManager,
            IEventPublisher eventPublisher,
            IWebServer webServer,
            IEnumerable<IWebBrowser> webBrowsers,
            string xapPath,
            IDialogMonitorRunner dialogMonitorRunner)
        {
            _logger = logger;
            _eventSubscriptionManager = eventSubscriptionManager;
            _webServer = webServer;
            _webBrowsers = webBrowsers.ToList();
            _xapPath = xapPath;
            _dialogMonitorRunner = dialogMonitorRunner;

            _testResultAggregator = new TestResultAggregator(logger, eventPublisher, _xapPath);
            _eventSubscriptionManager.AddListener(_testResultAggregator);
            _eventSubscriptionManager.AddListener(this);
        }

        public virtual TestReport Run()
        {
            _logger.Information("{1}{1}Starting Test Run: {0}{1}{1}"
                .FormatWith(DateTime.Now, Environment.NewLine));

            _webServer.Start();
            foreach(var browser in _webBrowsers)
                browser.Start();
            _dialogMonitorRunner.Start();

            _browserThreadWaitHandle.WaitOne();

            _dialogMonitorRunner.Stop();
            foreach (var browser in _webBrowsers)
                browser.Stop();
            _webServer.Stop();

            var testReport = _testResultAggregator.CurrentReport;
            return testReport;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var browser in _webBrowsers)
                    browser.Dispose();
                _eventSubscriptionManager.RemoveListener(this);
                _eventSubscriptionManager.RemoveListener(_testResultAggregator);
                _browserThreadWaitHandle.Close();
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
            _browserThreadWaitHandle.Set();
        }
    }
}
