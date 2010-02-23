using System;

namespace StatLight.Client.Harness.Events
{
    public class ExceptionInfo
    {
        public string Message { get; private set; }

        public string StackTrace { get; private set; }

        public string FullMessage { get; private set; }

        public ExceptionInfo InnerException { get; private set; }

        internal ExceptionInfo() { }

        public ExceptionInfo(Exception exception)
        {
            if (exception == null)
                throw new ArgumentNullException("exception");

            if (exception.InnerException != null)
                InnerException = new ExceptionInfo(exception.InnerException);
            Message = exception.Message;
            StackTrace = exception.StackTrace;
            FullMessage = exception.ToString();
        }

        public override string ToString()
        {
            return FullMessage;
        }

        public static implicit operator ExceptionInfo(Exception ex)
        {
            return new ExceptionInfo(ex);
        }
    }
}