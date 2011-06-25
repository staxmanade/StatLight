
namespace StatLight.Core.Runners
{
    using System.Collections.Generic;
    using StatLight.Core.Common;
    using EventAggregatorNet;
    using StatLight.Core.Monitoring;
    using StatLight.Core.Reporting;
    using StatLight.Core.Reporting.Providers.TeamCity;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer;

    internal class TeamCityRunner : OnetimeRunner
    {
        private readonly IEventSubscriptionManager _eventSubscriptionManager;
        readonly TeamCityTestResultHandler _teamCityTestResultHandler;

        internal TeamCityRunner(
            ILogger logger,
            IEventSubscriptionManager eventSubscriptionManager, 
            IEventPublisher eventPublisher,
            IWebServer webServer,
            List<IWebBrowser> webBrowsers,
            TeamCityTestResultHandler teamCityTestResultHandler,
            string xapPath,
            IDialogMonitorRunner dialogMonitorRunner)
            : base(logger, eventSubscriptionManager, eventPublisher, webServer, webBrowsers, xapPath, dialogMonitorRunner)
        {
            _eventSubscriptionManager = eventSubscriptionManager;
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
