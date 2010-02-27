namespace StatLight.Core.Events
{
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Events.Aggregation;

    public sealed class DialogAssertionEvent
    {
        public string ExceptionMessage { get; set; }
    }

    public sealed class TestRunCompletedEvent { }
    public sealed class BrowserHostCommunicationTimeoutEvent { }

    public class MessageReceivedFromClientServerEvent { }

    public interface ITestingReportEvents :
        IListener<TestExecutionMethodPassedClientEvent>,
        IListener<TestExecutionMethodFailedClientEvent>,
        IListener<TestExecutionMethodIgnoredClientEvent>,
        IListener<DialogAssertionEvent>,
        IListener<TraceClientEvent>,
        IListener<BrowserHostCommunicationTimeoutEvent>
    { }
}
