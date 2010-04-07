using System;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;
using StatLight.Core.Common;

namespace StatLight.Core.Reporting.Providers.Console
{
    public class ConsoleResultHandler : ITestingReportEvents
    {
        private readonly ILogger _logger;

        public ConsoleResultHandler(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(TraceClientEvent message)
        {
            _logger.Warning(Environment.NewLine);
            _logger.Warning(message.Message);
            _logger.Warning(Environment.NewLine);
        }

        public void Handle(DialogAssertionServerEvent message)
        {
            WriteString(message.Message);
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
            WriteString(message.Message);
        }

        private void WriteString(string message)
        {
            _logger.Warning(Environment.NewLine);
            _logger.Warning(message);
            _logger.Warning(Environment.NewLine);
        }

        public void Handle(TestCaseResult message)
        {
            switch (message.ResultType)
            {
                case ResultType.Ignored:
                    "I".WrapConsoleMessageWithColor(ConsoleColor.Yellow, false);
                    break;
                case ResultType.Passed:
                    ".".WrapConsoleMessageWithColor(ConsoleColor.White, false);
                    break;
                case ResultType.Failed:
                case ResultType.SystemGeneratedFailure:
                    System.Console.WriteLine("");
                    "------------------ ".WrapConsoleMessageWithColor(ConsoleColor.DarkRed, false);
                    "Test ".WrapConsoleMessageWithColor(ConsoleColor.DarkRed, false);
                    "Failed".WrapConsoleMessageWithColor(ConsoleColor.Red, false);
                    " ------------------".WrapConsoleMessageWithColor(ConsoleColor.DarkRed, true);

                    "Test Class:  ".WrapConsoleMessageWithColor(ConsoleColor.White, false);
                    message.ClassName.WrapConsoleMessageWithColor(ConsoleColor.Red, true);

                    "Test Method: ".WrapConsoleMessageWithColor(ConsoleColor.White, false);
                    message.MethodName.WrapConsoleMessageWithColor(ConsoleColor.Red, true);

                    if (!string.IsNullOrEmpty(message.OtherInfo))
                    {
                        "Other Info: ".WrapConsoleMessageWithColor(ConsoleColor.White, false);
                        message.OtherInfo.WrapConsoleMessageWithColor(ConsoleColor.Red, true);
                    }

                    if (message.ExceptionInfo != null)
                    {
                        //TODO: print to the console - the exception info in a more readable/visually parsable format
                        "Exception Message: ".WrapConsoleMessageWithColor(ConsoleColor.White, true);
                        message.ExceptionInfo.FullMessage.WrapConsoleMessageWithColor(ConsoleColor.Red, true);
                    }

                    "-------------------------------------------------"
                        .WrapConsoleMessageWithColor(ConsoleColor.DarkRed, true);
                    break;

                default:
                    "Unknown TestCaseResult (to StatLight) - {0}".FormatWith(message.ResultType)
                        .WrapConsoleMessageWithColor(ConsoleColor.Red, true);
                    break;
            }
        }
    }
}
