using System;
using System.Linq;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;
using StatLight.Core.Common;
using StatLight.Core.Properties;

namespace StatLight.Core.Reporting.Providers.Console
{
    public class ConsoleResultHandler : ITestingReportEvents
    {
        private readonly ILogger _logger;
        private readonly Settings _settings;

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

        public void Handle(TestCaseResult message)
        {
            if (message == null) throw new ArgumentNullException("message");
            switch (message.ResultType)
            {
                case ResultType.Ignored:
                    "I".WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, false);
                    break;
                case ResultType.Passed:
                    ".".WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, false);
                    break;
                case ResultType.Failed:
                case ResultType.SystemGeneratedFailure:
                    System.Console.WriteLine("");
                    "------------------ ".WrapConsoleMessageWithColor(_settings.ConsoleColorError, false);
                    "Test ".WrapConsoleMessageWithColor(_settings.ConsoleColorError, false);
                    "Failed".WrapConsoleMessageWithColor(_settings.ConsoleColorError, false);
                    " ------------------".WrapConsoleMessageWithColor(_settings.ConsoleColorError, true);

                    "Test Namespace:    ".WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, false);
                    message.NamespaceName.WrapConsoleMessageWithColor(_settings.ConsoleColorError, true);

                    "Test Class:        ".WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, false);
                    message.ClassName.WrapConsoleMessageWithColor(_settings.ConsoleColorError, true);

                    "Test Method:       ".WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, false);
                    message.MethodName.WrapConsoleMessageWithColor(_settings.ConsoleColorError, true);

                    if (!string.IsNullOrEmpty(message.OtherInfo))
                    {
                        "Other Info:        ".WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, false);
                        message.OtherInfo.WrapConsoleMessageWithColor(_settings.ConsoleColorError, true);
                    }

                    foreach (var metaData in message.Metadata)
                    {
                        string.Format("{0,-19}", metaData.Classification + ": ").WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, false);
                        (metaData.Name + " - ").WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, false);
                        metaData.Value.WrapConsoleMessageWithColor(_settings.ConsoleColorError, true);
                    }

                    WriteExceptionInfo(message.ExceptionInfo);

                    "-------------------------------------------------"
                        .WrapConsoleMessageWithColor(ConsoleColor.DarkRed, true);
                    break;

                default:
                    "Unknown TestCaseResult (to StatLight) - {0}".FormatWith(message.ResultType)
                        .WrapConsoleMessageWithColor(_settings.ConsoleColorError, true);
                    break;
            }
        }

        private void WriteExceptionInfo(ExceptionInfo exceptionInfo)
        {
            if (exceptionInfo != null)
            {
                //TODO: print to the console - the exception info in a more readable/visually parsable format
                "Exception Message: ".WrapConsoleMessageWithColor(_settings.ConsoleColorInformation, true);
                exceptionInfo.FullMessage.WrapConsoleMessageWithColor(_settings.ConsoleColorError, true);
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
    }
}
