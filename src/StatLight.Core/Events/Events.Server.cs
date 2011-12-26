
namespace StatLight.Core.Events
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
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

    [DebuggerDisplay("Result=[{ResultType}], Method={NamespaceName}.{ClassName}.{MethodName}")]
    public class TestCaseResultServerEvent
    {
        private readonly List<MetaDataInfo> _metadata;

        public TestCaseResultServerEvent(ResultType resultType)
        {
            ResultType = resultType;
            _metadata = new List<MetaDataInfo>();
        }

        public string NamespaceName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Finished { get; set; }
        public string OtherInfo { get; set; }

        public void PopulateMetadata(IEnumerable<MetaDataInfo> metadata)
        {
            if (metadata == null) throw new ArgumentNullException("metadata");
            _metadata.AddRange(metadata);
        }

        public IEnumerable<MetaDataInfo> Metadata { get { return _metadata; } }

        public TimeSpan TimeToComplete
        {
            get
            {
                if (Finished.HasValue)
                    return Finished.Value - Started;

                return new TimeSpan();
            }
        }
        public ExceptionInfo ExceptionInfo { get; set; }

        public ResultType ResultType { get; private set; }

        public string FullMethodName()
        {
            const string delimiter = ".";
            return (NamespaceName ?? string.Empty) + delimiter +
                   (ClassName ?? string.Empty) + delimiter +
                   (MethodName ?? string.Empty);
        }
    }
}
