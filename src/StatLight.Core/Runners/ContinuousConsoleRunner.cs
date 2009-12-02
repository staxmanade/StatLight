
namespace StatLight.Core.Runners
{
	using System;
	using System.Globalization;
	using System.Threading;
	using StatLight.Core.Common;
	using StatLight.Core.Reporting;
	using StatLight.Core.WebBrowser;
	using StatLight.Core.WebServer;
using Microsoft.Practices.Composite.Events;

	internal class ContinuousConsoleRunner : IRunner, IDisposable
	{
		private readonly IWebServer webServer;
		private readonly Thread continuousRunnerThread;
		private readonly XapFileBuildChangedMonitor xapFileBuildChangedMonitor;

		internal ContinuousConsoleRunner(
			ILogger logger,
			IEventAggregator eventAggregator,
			string xapPath,
			IStatLightService statLightService,
			IWebServer webServer,
			IBrowserFormHost browserFormHost,
			ITestResultHandler testResultHandler)
		{
			this.webServer = webServer;
			xapFileBuildChangedMonitor = new XapFileBuildChangedMonitor(xapPath);

			this.continuousRunnerThread = new Thread(() =>
			{
				var runner = new ContinuousTestRunner(logger, eventAggregator, browserFormHost, statLightService, xapFileBuildChangedMonitor, testResultHandler);
				string line;
				while ((line = System.Console.ReadLine()).ToLower(CultureInfo.CurrentCulture) != "exit")
				{
					runner.ForceFilteredTest(line);
				}
			});
		}

		public TestReport Run()
		{
			webServer.Start();

			continuousRunnerThread.Start();
			continuousRunnerThread.Join();

			return new TestReport();
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				xapFileBuildChangedMonitor.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
