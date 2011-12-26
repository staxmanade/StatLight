namespace StatLight.Core.Reporting.Providers.Console
{
    using System;
    using Events;
    using StatLight.Core.Common;
    using StatLight.Core.Properties;

    public class ConsoleResultHandler : ITestingReportEvents,
        IListener<TestReportGeneratedServerEvent>,
        IListener<TestReportCollectionGeneratedServerEvent>
    {
        private readonly ILogger _logger;
        private static Settings _settings;
        private static Settings Settings
        {
            get
            {
                return (_settings ?? (_settings = Settings.Default));
            }
        }

        public ConsoleResultHandler(ILogger logger)
            : this(logger, Settings.Default)
        {
        }

        public ConsoleResultHandler(ILogger logger, Settings settings)
        {
            _logger = logger;
            _settings = settings;
        }

        public void Handle(TraceClientEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _logger.Warning(Environment.NewLine);
            _logger.Warning(message.Message);
            _logger.Warning(Environment.NewLine);
        }

        public void Handle(DialogAssertionServerEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            WriteString(message.Message);
        }

        public void Handle(BrowserHostCommunicationTimeoutServerEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            WriteString(message.Message);
        }

        private void WriteString(string message)
        {
            _logger.Warning(Environment.NewLine);
            _logger.Warning(message);
            _logger.Warning(Environment.NewLine);
        }

        public void Handle(TestCaseResultServerEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            switch (message.ResultType)
            {
                case ResultType.Ignored:
                    "I".WrapConsoleMessageWithColor(Settings.ConsoleColorInformation, false);
                    break;
                case ResultType.Passed:
                    ".".WrapConsoleMessageWithColor(Settings.ConsoleColorInformation, false);
                    break;
                case ResultType.Failed:
                case ResultType.SystemGeneratedFailure:
                    WriteOutError(message);
                    break;

                default:
                    "Unknown TestCaseResultServerEvent (to StatLight) - {0}".FormatWith(message.ResultType)
                        .WrapConsoleMessageWithColor(Settings.ConsoleColorError, true);
                    break;
            }
        }

        public static void WriteOutError(TestCaseResultServerEvent message)
        {
            if (message == null)
                throw new ArgumentNullException("message");

            System.Console.WriteLine("");
            "------------------ ".WrapConsoleMessageWithColor(Settings.ConsoleColorError, false);
            "Test ".WrapConsoleMessageWithColor(Settings.ConsoleColorError, false);
            "Failed".WrapConsoleMessageWithColor(Settings.ConsoleColorError, false);
            " ------------------".WrapConsoleMessageWithColor(Settings.ConsoleColorError, true);

            "Test Namespace:    ".WrapConsoleMessageWithColor(Settings.ConsoleColorInformation, false);
            message.NamespaceName.WrapConsoleMessageWithColor(Settings.ConsoleColorError, true);

            "Test Class:        ".WrapConsoleMessageWithColor(Settings.ConsoleColorInformation, false);
            message.ClassName.WrapConsoleMessageWithColor(Settings.ConsoleColorError, true);

            "Test Method:       ".WrapConsoleMessageWithColor(Settings.ConsoleColorInformation, false);
            message.MethodName.WrapConsoleMessageWithColor(Settings.ConsoleColorError, true);

            if (!string.IsNullOrEmpty(message.OtherInfo))
            {
                "Other Info:        ".WrapConsoleMessageWithColor(Settings.ConsoleColorInformation, false);
                message.OtherInfo.WrapConsoleMessageWithColor(Settings.ConsoleColorError, true);
            }

            foreach (var metaData in message.Metadata)
            {
                "{0,-19}".FormatWith(metaData.Classification + ": ").WrapConsoleMessageWithColor(
                    Settings.ConsoleColorInformation, false);
                (metaData.Name + " - ").WrapConsoleMessageWithColor(Settings.ConsoleColorInformation, false);
                metaData.Value.WrapConsoleMessageWithColor(Settings.ConsoleColorError, true);
            }

            WriteExceptionInfo(message.ExceptionInfo);

            "-------------------------------------------------"
                .WrapConsoleMessageWithColor(ConsoleColor.DarkRed, true);
        }

        private static void WriteExceptionInfo(ExceptionInfo exceptionInfo)
        {
            if (exceptionInfo != null)
            {
                //TODO: print to the console - the exception info in a more readable/visually parsable format
                "Exception Message: ".WrapConsoleMessageWithColor(Settings.ConsoleColorInformation, true);
                exceptionInfo.FullMessage.WrapConsoleMessageWithColor(Settings.ConsoleColorError, true);
            }
        }

        public void Handle(FatalSilverlightExceptionServerEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            WriteString(message.Message);
        }

        public void Handle(UnhandledExceptionClientEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            WriteExceptionInfo(message.ExceptionInfo);
        }

        public void Handle(TestReportGeneratedServerEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");

            if (message.ShouldPrintSummary)
            {
                ConsoleTestCompleteMessage.WriteOutCompletionStatement(message.TestReport, message.ElapsedTimeOfRun);
            }
        }

        public void Handle(TestReportCollectionGeneratedServerEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");

            ConsoleTestCompleteMessage.PrintFinalTestSummary(message.TestReportCollection, message.TotalTime);
        }
    }
}
