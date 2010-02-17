using System;
using StatLight.Core.Reporting.Messages;
using StatLight.Core.Events.Aggregation;
using StatLight.Client.Model.Events;
using StatLight.Core.Common;

namespace StatLight.Core.Reporting.Providers.Console
{
    internal class ConsoleResultHandler : ITestResultHandler
    {
        private readonly ILogger _logger;

        public ConsoleResultHandler(ILogger logger, IEventAggregator eventAggregator)
        {
            _logger = logger;

            eventAggregator.AddListener<TraceClientEvent>((e) =>
              {
                  _logger.Warning(Environment.NewLine);
                  _logger.Warning(e.Message);
                  _logger.Warning(Environment.NewLine);
              });
        }

        public void HandleMessage(MobilScenarioResult result)
        {
            if (result.Result == TestOutcome.Passed)
            {
                ".".WrapConsoleMessageWithColor(System.ConsoleColor.White, false);
            }
            else
            {
                System.Console.WriteLine("");
                "------------------ ".WrapConsoleMessageWithColor(System.ConsoleColor.DarkRed, false);
                "Test ".WrapConsoleMessageWithColor(System.ConsoleColor.DarkRed, false);
                result.Result.ToString().WrapConsoleMessageWithColor(System.ConsoleColor.Red, false);
                " ------------------".WrapConsoleMessageWithColor(System.ConsoleColor.DarkRed, true);

                "Test Class:  ".WrapConsoleMessageWithColor(System.ConsoleColor.White, false);
                result.TestClassName.WrapConsoleMessageWithColor(System.ConsoleColor.Red, true);

                "Test Method: ".WrapConsoleMessageWithColor(System.ConsoleColor.White, false);
                result.TestName.WrapConsoleMessageWithColor(System.ConsoleColor.Red, true);

                "Exception Message: ".WrapConsoleMessageWithColor(System.ConsoleColor.White, true);
                result.ExceptionMessage.WrapConsoleMessageWithColor(System.ConsoleColor.Red, true);

                "-------------------------------------------------".WrapConsoleMessageWithColor(System.ConsoleColor.DarkRed, true);
            }
        }

        public void HandleMessage(MobilOtherMessageType result)
        {
            if (result.IsIgnoreMessage())
            {
                "I".WrapConsoleMessageWithColor(System.ConsoleColor.Yellow, false);
            }
            else if (result.IsBrowserCommErrorMessage())
                result.TraceMessage().WrapConsoleMessageWithColor(System.ConsoleColor.Red, true);
        }
    }
}
