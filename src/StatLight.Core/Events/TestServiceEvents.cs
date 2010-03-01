namespace StatLight.Core.Events
{
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Events.Aggregation;

    public sealed class DialogAssertionServerEvent
    {
        public string Message { get; set; }
    }

    public sealed class TestRunCompletedServerEvent { }
    public sealed class BrowserHostCommunicationTimeoutServerEvent
    {
        public string Message { get; set; }
    }

    public class MessageReceivedFromClientServerEvent { }

    public interface ITestingReportEvents :
        IListener<TestExecutionMethodPassedClientEvent>,
        IListener<TestExecutionMethodFailedClientEvent>,
        IListener<TestExecutionMethodIgnoredClientEvent>,
        IListener<TraceClientEvent>,
        IListener<DialogAssertionServerEvent>,
        IListener<BrowserHostCommunicationTimeoutServerEvent>
    { }
}
