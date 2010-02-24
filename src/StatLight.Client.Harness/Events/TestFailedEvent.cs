using System;
using System.Threading;

namespace StatLight.Client.Harness.Events
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

    #region Test infrastructure events
    public class InitializationOfUnitTestHarnessClientEvent : ClientEvent { }
    public class SignalTestCompleteClientEvent : ClientEvent
    {
        public int TotalMessagesPostedCount { get; set; }
    }



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
        public TimeSpan TimeToComplete
        {
            get { return Finished - Started; }
        }
    }

    public class TestExecutionMethodFailedClientEvent : TestExecutionMethod
    {
        public ExceptionInfo ExceptionInfo { get; set; }
        public DateTime Started { get; set; }
        public DateTime Finished { get; set; }
        public TimeSpan TimeToComplete
        {
            get { return Finished - Started; }
        }
    }

    public class TestExecutionMethodPassedClientEvent : TestExecutionMethod
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
        public ExceptionInfo Exception { get; set; }
    }

    public class TraceClientEvent : ClientEvent
    {
        public string Message { get; set; }
    }

    #endregion
}