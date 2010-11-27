
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
        readonly TeamCityTestResultHandler _teamCityTestResultHandler;

        internal TeamCityRunner(
            ILogger logger,
            IEventAggregator eventAggregator,
            IWebServer webServer,
            List<IWebBrowser> webBrowsers,
            TeamCityTestResultHandler teamCityTestResultHandler,
            string xapPath,
            IDialogMonitorRunner dialogMonitorRunner)
            : base(logger, eventAggregator, webServer, webBrowsers, xapPath, dialogMonitorRunner)
        {
            _eventSubscriptionManager = eventAggregator;
            _teamCityTestResultHandler = teamCityTestResultHandler;
        }

        public override TestReport Run()
        {
            _eventSubscriptionManager.AddListener(_teamCityTestResultHandler);

            _teamCityTestResultHandler.PublishStart();

            var testReport = base.Run();

            _teamCityTestResultHandler.PublishStop();

            _eventSubscriptionManager.RemoveListener(_teamCityTestResultHandler);
            return testReport;
        }
    }
}
