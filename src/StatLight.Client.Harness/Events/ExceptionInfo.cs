using System;
using System.Text.RegularExpressions;

namespace StatLight.Client.Harness.Events
{
    public class ExceptionInfo
    {
        public string Message { get; set; }

        // FYI: Silverlight doesn't have a 'Source' property.
        //public string Source { get; set; }

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
            Message = FixupErrorMessage(exception.Message);
            StackTrace = exception.StackTrace;
            FullMessage = FixupErrorMessage(exception.ToString());
        }

        public override string ToString()
        {
            return FullMessage;
        }

        public static implicit operator ExceptionInfo(Exception ex)
        {
            return new ExceptionInfo(ex);
        }

        // The MSTest assertion messages can get a little funny looking. So we're going to try to pretty them up a bit
        private static readonly Regex CleanupSearchRegex = new Regex("Expected:&lt;(.*)&gt;. Actual:&lt;(.*)&gt;.");

        private static string FixupErrorMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                return message;

            return CleanupSearchRegex.Replace(message, "Expected:<$1>. Actual:<$2>.");
        }

    }
}