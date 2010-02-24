

namespace StatLight.Core.Events
{
    using StatLight.Core.Reporting.Messages;

    //public sealed class TestResultEvent 
    //    : PayloadEvent<MobilScenarioResult> { }

    public sealed class DialogAssertionEvent 
        : PayloadEvent<MobilScenarioResult> { }


    public sealed class TestRunCompletedEvent { }
    public sealed class BrowserHostCommunicationTimeoutEvent { }

    public class MessageReceivedFromClientServerEvent {}
}
