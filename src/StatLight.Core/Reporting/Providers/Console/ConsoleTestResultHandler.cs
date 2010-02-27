using System;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;
using StatLight.Core.Events.Aggregation;
using StatLight.Core.Common;

namespace StatLight.Core.Reporting.Providers.Console
{
    public class ConsoleResultHandler : ITestingReportEvents
    {
        private readonly ILogger _logger;

        public ConsoleResultHandler(ILogger logger, IEventAggregator eventAggregator)
        {
            _logger = logger;
        }

        public void Handle(TestExecutionMethodPassedClientEvent message)
        {
            ".".WrapConsoleMessageWithColor(ConsoleColor.White, false);
        }

        public void Handle(TestExecutionMethodFailedClientEvent message)
        {
            System.Console.WriteLine("");
            "------------------ ".WrapConsoleMessageWithColor(ConsoleColor.DarkRed, false);
            "Test ".WrapConsoleMessageWithColor(ConsoleColor.DarkRed, false);
            "Failed".WrapConsoleMessageWithColor(ConsoleColor.Red, false);
            " ------------------".WrapConsoleMessageWithColor(ConsoleColor.DarkRed, true);

            "Test Class:  ".WrapConsoleMessageWithColor(ConsoleColor.White, false);
            message.ClassName.WrapConsoleMessageWithColor(ConsoleColor.Red, true);

            "Test Method: ".WrapConsoleMessageWithColor(ConsoleColor.White, false);
            message.MethodName.WrapConsoleMessageWithColor(ConsoleColor.Red, true);

            //TODO: print to the console - the exception info in a more readable/visually parsable format
            "Exception Message: ".WrapConsoleMessageWithColor(ConsoleColor.White, true);
            message.ExceptionInfo.FullMessage.WrapConsoleMessageWithColor(ConsoleColor.Red, true);

            "-------------------------------------------------".WrapConsoleMessageWithColor(ConsoleColor.DarkRed, true);
        }

        public void Handle(TestExecutionMethodIgnoredClientEvent message)
        {
            "I".WrapConsoleMessageWithColor(ConsoleColor.Yellow, false);
        }

        public void Handle(TraceClientEvent message)
        {
            _logger.Warning(Environment.NewLine);
            _logger.Warning(message.Message);
            _logger.Warning(Environment.NewLine);
        }

        public void Handle(DialogAssertionServerEvent message)
        {
            throw new NotImplementedException();
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
            throw new NotImplementedException();
        }
    }
}
