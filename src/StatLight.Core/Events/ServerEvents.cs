
namespace StatLight.Core.Events
{
    using System.ComponentModel.Composition;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Events;
    using StatLight.Core.Reporting;

    public enum DialogType
    {
        None,
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

    public sealed class TestReportGeneratedServerEvent
    {
        public TestReportGeneratedServerEvent(TestReport testReport)
        {
            TestReport = testReport;
        }
        public TestReport TestReport { get; set; }
    }

    public sealed class TestReportCollectionGeneratedServerEvent
    {
        public TestReportCollectionGeneratedServerEvent(TestReportCollection testReportCollection)
        {
            this.TestReportCollection = testReportCollection;
        }
        public TestReportCollection TestReportCollection { get; set; }
    }

    public class MessageReceivedFromClientServerEvent { }

    [InheritedExport]
    public interface ITestingReportEvents : IListener<TestCaseResult>,
        IListener<TraceClientEvent>,
        IListener<BrowserHostCommunicationTimeoutServerEvent>,
        IListener<FatalSilverlightExceptionServerEvent>,
        IListener<UnhandledExceptionClientEvent>
    { }

}
