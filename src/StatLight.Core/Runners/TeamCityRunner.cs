
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
        private readonly IEventSubscriptionManager _eventSubscriptionManager;
        readonly TeamCityTestResultHandler _teamCityCommandPublisher;

        internal TeamCityRunner(
            ILogger logger,
            IEventAggregator eventAggregator,
            IWebServer webServer,
            List<IWebBrowser> webBrowsers,
            TeamCityTestResultHandler teamCityCommandPublisher,
            string xapPath,
            IDialogMonitorRunner dialogMonitorRunner)
            : base(logger, eventAggregator, webServer, webBrowsers, xapPath, dialogMonitorRunner)
        {
            _eventSubscriptionManager = eventAggregator;
            _teamCityCommandPublisher = teamCityCommandPublisher;
        }

        public override TestReport Run()
        {
            _teamCityCommandPublisher.PublishStart();

            var testReport = base.Run();

            _teamCityCommandPublisher.PublishStop();

            return testReport;
        }

        protected override void Dispose(bool disposing)
        {
            _eventSubscriptionManager.RemoveListener(_teamCityCommandPublisher);
            base.Dispose(disposing);
        }
    }
}
