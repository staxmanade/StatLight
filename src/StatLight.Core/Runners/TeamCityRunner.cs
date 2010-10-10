
using System.Collections.Generic;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Monitoring;

namespace StatLight.Core.Runners
{
	using StatLight.Core.Common;
	using StatLight.Core.Reporting.Providers.TeamCity;
	using StatLight.Core.WebBrowser;
	using StatLight.Core.WebServer;
	using StatLight.Core.Reporting;

    internal class TeamCityRunner : OnetimeRunner
	{
        readonly TeamCityTestResultHandler teamCityCommandPublisher;

		internal TeamCityRunner(
			ILogger logger,
			IEventAggregator eventAggregator,
			IWebServer webServer,
			List<IWebBrowser> browserFormHost,
			TeamCityTestResultHandler teamCityCommandPublisher,
            string xapPath, 
            IDialogMonitorRunner dialogMonitorRunner)
			: base(logger, eventAggregator, webServer, browserFormHost, xapPath, dialogMonitorRunner)
		{
			this.teamCityCommandPublisher = teamCityCommandPublisher;
		}

		public override TestReport Run()
		{
			teamCityCommandPublisher.PublishStart();

			var testReport = base.Run();

			teamCityCommandPublisher.PublishStop();

			return testReport;
		}

	}
}
