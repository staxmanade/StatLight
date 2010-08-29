
using System.Collections.Generic;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Reporting.Providers.Console;

namespace StatLight.Core.Runners
{
    using System;
    using System.Threading;
    using StatLight.Core.Common;
    using StatLight.Core.Reporting;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;
    using StatLight.Core.Events;

    internal class OnetimeRunner : IRunner, IDisposable,
        IListener<TestRunCompletedServerEvent>
    {
        private readonly ILogger logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly IWebServer statLightServiceHost;
        private readonly List<IBrowserFormHost> browserFormHost;
        private readonly string _xapPath;
        private readonly TestResultAggregator testResultAggregator;
        readonly AutoResetEvent _browserThreadWaitHandle = new AutoResetEvent(false);


        internal OnetimeRunner(
            ILogger logger,
            IEventAggregator eventAggregator,
            IWebServer statLightServiceHost,
            List<IBrowserFormHost> browserFormHost,
            string xapPath)
        {
            this.logger = logger;
            _eventAggregator = eventAggregator;
            this.statLightServiceHost = statLightServiceHost;
            this.browserFormHost = browserFormHost;
            _xapPath = xapPath;

            testResultAggregator = new TestResultAggregator(logger, eventAggregator, _xapPath);
            eventAggregator.AddListener(testResultAggregator);
            eventAggregator.AddListener(this);
        }

        public virtual TestReport Run()
        {
            DateTime startOfRun = DateTime.Now;
            logger.Information("{1}{1}Starting Test Run: {0}{1}{1}"
                .FormatWith(DateTime.Now, Environment.NewLine));

            statLightServiceHost.Start();
            foreach(var browser in browserFormHost)
                browser.Start();
            _browserThreadWaitHandle.WaitOne();
            foreach (var browser in browserFormHost)
                browser.Stop();
            statLightServiceHost.Stop();

            var testReport = testResultAggregator.CurrentReport;
            ConsoleTestCompleteMessage.WriteOutCompletionStatement(testReport, startOfRun);
            return testReport;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var browser in browserFormHost)
                    browser.Dispose();
                _eventAggregator.RemoveListener(this);
                _browserThreadWaitHandle.Close();
                testResultAggregator.Dispose();
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
