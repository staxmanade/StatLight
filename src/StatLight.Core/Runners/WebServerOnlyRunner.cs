

namespace StatLight.Core.Runners
{
    using System;
    using System.Globalization;
    using System.Threading;
    using StatLight.Core.Common;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Reporting;
    using StatLight.Core.WebServer;

    internal class WebServerOnlyRunner : IRunner
    {
        private readonly IWebServer _webServer;
        private readonly Thread _continuousRunnerThread;
        private readonly Uri _testHtmlPageUrl;
        private readonly string _xapPath;
        private readonly ILogger _logger;
        private readonly TestResultAggregator _testResultAggregator;
        private readonly IEventAggregator _eventAggregator;

        internal WebServerOnlyRunner(
            ILogger logger,
            IEventAggregator eventAggregator,
            IWebServer webServer,
            Uri testHtmlPageUrl,
            string xapPath)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;
            _webServer = webServer;
            _testHtmlPageUrl = testHtmlPageUrl;
            _xapPath = xapPath;

            _testResultAggregator = new TestResultAggregator(logger, eventAggregator, _xapPath);
            _eventAggregator.AddListener(_testResultAggregator);
            _continuousRunnerThread = new Thread(() =>
            {
                string line;
                do
                {
                    line = Console.ReadLine();
                } while (line.ToLower(CultureInfo.CurrentCulture) != "exit");
                //string line;
                //while ((line = System.Console.ReadLine())
                //        .ToLower(CultureInfo.CurrentCulture) != "exit")
                //{
                //    ;
                //}
            });
        }

        public TestReport Run()
        {
            _webServer.Start();

            _logger.Information("Xap test web server started. You can browse to... {0}{1}".FormatWith(Environment.NewLine, _testHtmlPageUrl));

            _continuousRunnerThread.Start();
            _continuousRunnerThread.Join();

            return new TestReport(_xapPath);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _eventAggregator.RemoveListener(_testResultAggregator);
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
