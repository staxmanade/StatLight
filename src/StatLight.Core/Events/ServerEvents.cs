
using System;
using System.Diagnostics;

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
        public TestReportGeneratedServerEvent(TestReport testReport, TimeSpan elapsedTimeOfRun, bool shouldPrintSummary)
        {
            TestReport = testReport;
            ElapsedTimeOfRun = elapsedTimeOfRun;
            ShouldPrintSummary = shouldPrintSummary;
        }

        public TestReport TestReport { get; set; }
        public TimeSpan ElapsedTimeOfRun { get; set; }

        public bool ShouldPrintSummary { get; private set; }
    }

    public sealed class TestReportCollectionGeneratedServerEvent
    {
        public TestReportCollectionGeneratedServerEvent(TestReportCollection testReportCollection, TimeSpan totalTime)
        {
            TestReportCollection = testReportCollection;
            TotalTime = totalTime;
        }

        public TestReportCollection TestReportCollection { get; set; }
        public TimeSpan TotalTime { get; set; }
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
