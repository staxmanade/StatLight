

namespace StatLight.Core.Runners
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using StatLight.Core.Common;
    using StatLight.Core.Events;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.Console;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;

    internal class OnetimeRunner : IRunner,
        IListener<TestRunCompletedServerEvent>
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWebServer _statLightServiceHost;
        private readonly List<IBrowserFormHost> _browserFormHost;
        private readonly string _xapPath;
        private readonly TestResultAggregator _testResultAggregator;
        readonly AutoResetEvent _browserThreadWaitHandle = new AutoResetEvent(false);


        internal OnetimeRunner(
            ILogger logger,
            IEventAggregator eventAggregator,
            IWebServer statLightServiceHost,
            List<IBrowserFormHost> browserFormHost,
            string xapPath)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _statLightServiceHost = statLightServiceHost;
            _browserFormHost = browserFormHost;
            _xapPath = xapPath;

            _testResultAggregator = new TestResultAggregator(logger, eventAggregator, _xapPath);
            eventAggregator.AddListener(_testResultAggregator);
            eventAggregator.AddListener(this);
        }

        public virtual TestReport Run()
        {
            DateTime startOfRun = DateTime.Now;
            _logger.Information("{1}{1}Starting Test Run: {0}{1}{1}"
                .FormatWith(DateTime.Now, Environment.NewLine));

            _statLightServiceHost.Start();
            foreach(var browser in _browserFormHost)
                browser.Start();
            _browserThreadWaitHandle.WaitOne();
            foreach (var browser in _browserFormHost)
                browser.Stop();
            _statLightServiceHost.Stop();

            var testReport = _testResultAggregator.CurrentReport;
            ConsoleTestCompleteMessage.WriteOutCompletionStatement(testReport, startOfRun);
            return testReport;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var browser in _browserFormHost)
                    browser.Dispose();
                _eventAggregator.RemoveListener(this);
                _eventAggregator.RemoveListener(_testResultAggregator);
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
