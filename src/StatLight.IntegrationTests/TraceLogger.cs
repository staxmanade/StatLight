using System;
using System.Diagnostics;
using StatLight.Core.Common;

namespace StatLight.IntegrationTests
{
    public class TraceLogger : LoggerBase
    {
        public TraceLogger()
            : this(LogChatterLevels.Full)
        {
        }
        public TraceLogger(LogChatterLevels logChatterLevel)
            : base(logChatterLevel)
        {
        }

        public override void Information(string message)
        {
            Trace.WriteLine("Info: " + message);
        }

        public override void Warning(string message)
        {
            Trace.WriteLine("Warn: " + message);
        }

        public override void Error(string message)
        {
            Trace.WriteLine("Err: " + message);
        }

        public override void Debug(string message)
        {
            Debug(message, true);
        }

        public override void Debug(string message, bool writeNewLine)
        {
            if (writeNewLine)
                Trace.WriteLine("Debug: " + message + Environment.NewLine);
            else
                Trace.WriteLine("Debug: " + message);
        }
    }
}