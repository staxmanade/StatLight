
namespace StatLight.Core.Runners
{
	using StatLight.Core.Common;
	using StatLight.Core.Reporting.Providers.TeamCity;
	using StatLight.Core.WebBrowser;
	using StatLight.Core.WebServer;
	using StatLight.Core.Reporting;
	using Microsoft.Practices.Composite.Events;

	internal class TeamCityRunner : OnetimeRunner
	{
		TeamCityTestResultHandler teamCityCommandPublisher;

		internal TeamCityRunner(
			ILogger logger,
			IEventAggregator eventAggregator,
			IWebServer webServer,
			IBrowserFormHost browserFormHost,
			TeamCityTestResultHandler teamCityCommandPublisher)
			: base(logger, eventAggregator, webServer, browserFormHost, teamCityCommandPublisher)
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
