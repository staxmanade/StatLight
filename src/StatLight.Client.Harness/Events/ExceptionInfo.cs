using System;

namespace StatLight.Client.Harness.Events
{
    public class ExceptionInfo
    {
        public string Message { get; set; }

        // FYI: Silverlight doesn't have a 'Source' property.
        public string Source { get; set; }

        public string StackTrace { get; set; }

        public string FullMessage { get; set; }

        public ExceptionInfo InnerException { get; set; }

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