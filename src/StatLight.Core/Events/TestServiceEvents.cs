namespace StatLight.Core.Events
{
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Reporting;

    public enum DialogType
    {
        Assert,
        MessageBox,
    }

    public sealed class FatalSilverlightExceptionServerEvent
    {
        public FatalSilverlightExceptionServerEvent(DialogType dialogType)
        {
            DialogType = dialogType;
        }

        public DialogType DialogType { get; private set; }

        public string Message { get; set; }
    }

    public sealed class DialogAssertionServerEvent
    {
        public DialogAssertionServerEvent(DialogType dialogType)
        {
            DialogType = dialogType;
        }

        public DialogType DialogType { get; private set; }

        public string Message { get; set; }
    }

    public sealed class XapFileBuildChangedServerEvent
    {
        public string XapPath { get; set; }

        public XapFileBuildChangedServerEvent(string xapPath)
        {
            XapPath = xapPath;
        }
    }
    public sealed class TestRunCompletedServerEvent { }
    public sealed class BrowserHostCommunicationTimeoutServerEvent
    {
        public string Message { get; set; }
    }

    public class MessageReceivedFromClientServerEvent { }

    public interface ITestingReportEvents : IListener<TestCaseResult>,
        IListener<TraceClientEvent>,
        IListener<BrowserHostCommunicationTimeoutServerEvent>,
        IListener<FatalSilverlightExceptionServerEvent>
    { }

}
