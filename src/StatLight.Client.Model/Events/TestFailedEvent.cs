using System;
using StatLight.Client.Model.DTO;

namespace StatLight.Client.Model.Events
{
    public abstract class ClientEvent
    {
        public int ClientEventOrder { get; set; }
        public DateTime ClientEventCreatedTime { get; set; }
    }

    public class TraceEvent : ClientEvent
    {
        public string Message { get; set; }
    }

    public class InitializationOfUnitTestHarness : ClientEvent { }



    public abstract class TestExecutionClass : ClientEvent
    {
        public string NamespaceName { get; set; }
        public string ClassName { get; set; }
    }

    public class TestExecutionClassBeginEvent : TestExecutionClass
    { }

    public class TestExecutionClassCompletedEvent : TestExecutionClass
    { }

    public abstract class TestExecutionMethod : TestExecutionClass
    {
        public string MethodName { get; set; }
    }

    public class TestExecutionMethodBeginEvent : TestExecutionMethod
    {
        public DateTime Started { get; set; }
    }

    public class TestExecutionMethodIgnoredEvent : TestExecutionMethod
    {
        public string Message { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
    }

    public class TestExecutionMethodFailedEvent : TestExecutionMethod
    {
        public ExceptionInfo ExceptionInfo { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
    }

    public class TestExecutionMethodPassedEvent : TestExecutionMethod
    {
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
    }

}