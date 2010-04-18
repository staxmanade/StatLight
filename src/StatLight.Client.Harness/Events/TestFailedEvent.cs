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
        public bool Failed { get; set; }
        public int TotalFailureCount { get; set; }
        public int TotalTestsCount { get; set; }
    }



    public abstract class TestExecutionClass : ClientEvent
    {
        public string NamespaceName { get; set; }
        public string ClassName { get; set; }

        public bool Equals(TestExecutionClass other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(other.NamespaceName, NamespaceName) && Equals(other.ClassName, ClassName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != typeof (TestExecutionClass)) return false;
            return Equals((TestExecutionClass) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((NamespaceName != null ? NamespaceName.GetHashCode() : 0)*397) ^ (ClassName != null ? ClassName.GetHashCode() : 0);
            }
        }
    }

    public class TestExecutionClassBeginClientEvent : TestExecutionClass
    { }

    public class TestExecutionClassCompletedClientEvent : TestExecutionClass
    { }

    public abstract class TestExecutionMethod : TestExecutionClass
    {
        public string MethodName { get; set; }

        public bool Equals(TestExecutionMethod other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(other.MethodName, MethodName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return Equals(obj as TestExecutionMethod);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ (MethodName != null ? MethodName.GetHashCode() : 0);
            }
        }

        //public string FullMethodName
        //{
        //    get { return "{0}.{1}.{2}".FormatWith(NamespaceName, ClassName, MethodName); }
        //}
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

    public class DebugClientEvent : ClientEvent
    {
        public string Message { get; set; }
    }

    #endregion
}
