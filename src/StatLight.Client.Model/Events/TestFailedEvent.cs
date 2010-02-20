using System;
using System.Threading;
using StatLight.Client.Model.DTO;

namespace StatLight.Client.Model.Events
{
    public abstract class ClientEvent
    {
        private static int _currentEventCreationOrder;

        protected ClientEvent()
        {
            Interlocked.Increment(ref _currentEventCreationOrder);
            ClientEventOrder = _currentEventCreationOrder;
            ClientEventCreatedTime = DateTime.Now;
        }

        public int ClientEventOrder { get; private set; }
        public DateTime ClientEventCreatedTime { get; private set; }
    }

    public class TraceClientEvent : ClientEvent
    {
        public string Message { get; set; }
    }

    public class InitializationOfUnitTestHarnessClientEvent : ClientEvent { }



    public abstract class TestExecutionClass : ClientEvent
    {
        public string NamespaceName { get; set; }
        public string ClassName { get; set; }
    }

    public class TestExecutionClassBeginClientEvent : TestExecutionClass
    { }

    public class TestExecutionClassCompletedClientEvent : TestExecutionClass
    { }

    public abstract class TestExecutionMethod : TestExecutionClass
    {
        public string MethodName { get; set; }
    }

    public class TestExecutionMethodBeginClientEvent : TestExecutionMethod
    {
        public DateTime Started { get; set; }
    }

    public class TestExecutionMethodIgnoredClientEvent : TestExecutionMethod
    {
        public string Message { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
    }

    public class TestExecutionMethodFailedClientEvent : TestExecutionMethod
    {
        public ExceptionInfo ExceptionInfo { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
    }

    public class TestExecutionMethodPassedClientEvent : TestExecutionMethod
    {
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
    }

}