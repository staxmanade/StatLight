
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Runners
{
	using System;
	using System.Threading;
	using StatLight.Core.Common;
	using StatLight.Core.Reporting;
	using StatLight.Core.WebBrowser;
	using StatLight.Core.WebServer;

    internal class ContinuousConsoleRunner : IRunner, IDisposable
	{
		private readonly IWebServer _webServer;
		private readonly Thread _continuousRunnerThread;
		private readonly XapFileBuildChangedMonitor _xapFileBuildChangedMonitor;

		internal ContinuousConsoleRunner(
			ILogger logger,
			IEventAggregator eventAggregator,
			string xapPath,
			IStatLightService statLightService,
			IWebServer webServer,
			IBrowserFormHost browserFormHost)
		{
			_webServer = webServer;
			_xapFileBuildChangedMonitor = new XapFileBuildChangedMonitor(xapPath);

            _continuousRunnerThread = new Thread(() =>
            {
                using (var runner = new ContinuousTestRunner(logger, eventAggregator, browserFormHost, statLightService, _xapFileBuildChangedMonitor))
                {
                    string line;
                    while ( !(line = System.Console.ReadLine()).Equals("exit", StringComparison.OrdinalIgnoreCase))
                    {
                        runner.ForceFilteredTest(line);
                    }
                }
            });
		}

		public TestReport Run()
		{
			_webServer.Start();

			_continuousRunnerThread.Start();
			_continuousRunnerThread.Join();

			return new TestReport();
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
	}
}
