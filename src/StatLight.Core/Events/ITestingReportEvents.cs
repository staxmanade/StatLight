namespace StatLight.Core.Events
{
    public interface ITestingReportEvents : IShouldBeAddedToEventAggregator,
                                            IListener<TestCaseResultServerEvent>,
                                            IListener<TraceClientEvent>,
                                            IListener<BrowserHostCommunicationTimeoutServerEvent>,
                                            IListener<FatalSilverlightExceptionServerEvent>,
                                            IListener<UnhandledExceptionClientEvent>
    { }
}