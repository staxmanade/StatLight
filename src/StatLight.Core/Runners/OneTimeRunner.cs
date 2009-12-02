
namespace StatLight.Core.Runners
{
	using System;
	using System.Threading;
	using StatLight.Core.Common;
	using StatLight.Core.Reporting;
	using StatLight.Core.WebBrowser;
	using StatLight.Core.WebServer;
	using Microsoft.Practices.Composite.Events;
	using StatLight.Core.Events;

	internal class OnetimeRunner : IRunner, IDisposable
	{
		private readonly ILogger logger;
		private readonly IWebServer statLightServiceHost;
		private readonly IBrowserFormHost browserFormHost;
		private readonly TestResultAggregator testResultAggregator;
		AutoResetEvent _browserThreadWaitHandle = new AutoResetEvent(false);


		internal OnetimeRunner(
			ILogger logger,
			IEventAggregator eventAggregator,
			IWebServer statLightServiceHost,
			IBrowserFormHost browserFormHost,
			ITestResultHandler testResultHandler)
		{
			this.logger = logger;
			this.statLightServiceHost = statLightServiceHost;
			this.browserFormHost = browserFormHost;

			this.testResultAggregator = new TestResultAggregator(testResultHandler, eventAggregator);

			eventAggregator
				.GetEvent<TestRunCompletedEvent>()
				.Subscribe((payload) =>
							{
								_browserThreadWaitHandle.Set();
							});
		}

		public virtual TestReport Run()
		{

			logger.Information("{1}{1}Starting Test Run: {0}{1}{1}"
				.FormatWith(DateTime.Now, Environment.NewLine));

			this.statLightServiceHost.Start();
			this.browserFormHost.Start();
			_browserThreadWaitHandle.WaitOne();
			this.browserFormHost.Stop();
			this.statLightServiceHost.Stop();

			logger.Information("{1}{1}--- Completed Test Run: {0}{1}{1}"
				.FormatWith(DateTime.Now, Environment.NewLine));

			return this.testResultAggregator.CurrentReport;
		}

		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				testResultAggregator.Dispose();
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
	}
}
