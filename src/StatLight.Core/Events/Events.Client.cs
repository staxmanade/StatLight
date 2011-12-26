using System;
using System.Collections.ObjectModel;

namespace StatLight.Core.Events
{
    public abstract class ClientEvent
    {
        protected ClientEvent()
        {
            ClientEventCreatedTime = DateTime.Now;
        }

        public DateTime ClientEventCreatedTime { get; private set; }
    }

    #region Test infrastructure events
    public class InitializationOfUnitTestHarnessClientEvent : ClientEvent { }
    public class SignalTestCompleteClientEvent : ClientEvent
    {
        public int TotalMessagesPostedCount { get; set; }
        public bool Failed { get; set; }
        public int TotalFailureCount { get; set; }
        public int TotalTestsCount { get; set; }

        public string OtherInfo { get; set; }

        public int BrowserInstanceId { get; set; }
    }



    public abstract class TestExecutionClassClientEvent : ClientEvent
    {
        public string NamespaceName { get; set; }
        public string ClassName { get; set; }
    }

    public class TestExecutionClassBeginClientEvent : TestExecutionClassClientEvent
    { }

    public class TestExecutionClassCompletedClientEvent : TestExecutionClassClientEvent
    { }

    public class MetaDataInfo
    {
        public MetaDataInfo()
        {
        }

        public MetaDataInfo(string classification, string name, string value)
        {
            Classification = classification;
            Name = name;
            Value = value;
        }

        public string Classification { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public abstract class TestExecutionMethodClientEvent : TestExecutionClassClientEvent
    {
        private readonly Collection<MetaDataInfo> _metadata;

        protected TestExecutionMethodClientEvent()
        {
            _metadata = new Collection<MetaDataInfo>();
        }

        public string MethodName { get; set; }

        public Collection<MetaDataInfo> Metadata
        {
            get { return _metadata; }
        }

        public void AddMetadata(string classification, string name, string value)
        {
            _metadata.Add(new MetaDataInfo(classification: classification,
                                            name: name,
                                            value: value));
        }

        public string FullMethodName
        {
            get { return "{0}.{1}.{2}".FormatWith(NamespaceName, ClassName, MethodName); }
        }
    }

    public class TestExecutionMethodBeginClientEvent : TestExecutionMethodClientEvent
    {
        public DateTime Started { get; set; }
    }

    public class TestExecutionMethodIgnoredClientEvent : TestExecutionMethodClientEvent
    {
        public string Message { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public TimeSpan TimeToComplete
        {
            get { return Finished - Started; }
        }
    }

    public class TestExecutionMethodFailedClientEvent : TestExecutionMethodClientEvent
    {
        public ExceptionInfo ExceptionInfo { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public TimeSpan TimeToComplete
        {
            get { return Finished - Started; }
        }

        public string Description { get; set; }
    }

    public class TestExecutionMethodPassedClientEvent : TestExecutionMethodClientEvent
    {
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public TimeSpan TimeToComplete
        {
            get { return Finished - Started; }
        }
    }
    #endregion

    #region Non Test Result releated messages

    public class UnhandledExceptionClientEvent : ClientEvent
    {
        public ExceptionInfo ExceptionInfo { get; set; }
    }

    public class TraceClientEvent : ClientEvent
    {
        public string Message { get; set; }
    }

    public class TestContextMessageClientEvent : ClientEvent
    {
        public string FullTestName { get; set; }
        public string Message { get; set; }
        public int Order { get; set; }
    }

    public class DebugClientEvent : ClientEvent
    {
        public string Message { get; set; }
    }

    #endregion
}
