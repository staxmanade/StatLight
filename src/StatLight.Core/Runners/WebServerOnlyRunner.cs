﻿
namespace StatLight.Core.Runners
{
	using System;
	using System.Globalization;
	using System.Threading;
	using StatLight.Core.Common;
	using StatLight.Core.Reporting;
	using StatLight.Core.WebServer;
	using Microsoft.Practices.Composite.Events;

	internal class WebServerOnlyRunner : IRunner, IDisposable
	{
		private readonly IWebServer webServer;
		private readonly Thread continuousRunnerThread;
		private readonly Uri testHtmlPageUrl;
		private readonly ILogger logger;
		private readonly TestResultAggregator _testResultAggregator;

		internal WebServerOnlyRunner(
			ILogger logger,
			IEventAggregator eventAggregator,
			IWebServer webServer,
			Uri testHtmlPageUrl,
			ITestResultHandler testResultHandler)
		{
			this.logger = logger;
			this.webServer = webServer;
			this.testHtmlPageUrl = testHtmlPageUrl;

			this._testResultAggregator = new TestResultAggregator(testResultHandler, eventAggregator);

			this.continuousRunnerThread = new Thread(() =>
			{
				string line;
				do
				{
					line = System.Console.ReadLine();
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
			webServer.Start();

			logger.Information("Xap test web server started. You can browse to... {0}{1}".FormatWith(Environment.NewLine, testHtmlPageUrl));

			continuousRunnerThread.Start();
			continuousRunnerThread.Join();

			return new TestReport();
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