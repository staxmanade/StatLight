using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using writer = System.Console;

namespace StatLight.Core.Reporting.Providers.Console
{
    public static class ConsoleTestCompleteMessage
    {
        public static void WriteOutCompletionStatement(TestReport completeState, DateTime startOfRunTime)
        {
            writer.Write("{1}{1}--- Completed Test Run at: {0}. Total Run Time: {2}{1}{1}"
                .FormatWith(DateTime.Now, Environment.NewLine, DateTime.Now.Subtract(startOfRunTime)));

            writer.Write("Test run results: Total {0}, ", completeState.TotalResults);

            var successfulMessage = "Successful {0}, ".FormatWith(completeState.TotalPassed);
            if (completeState.TotalPassed > 0)
                successfulMessage.WrapConsoleMessageWithColor(ConsoleColor.Green, false);
            else
                writer.Write(successfulMessage);

            var failedMessage = "Failed {0}, ".FormatWith(completeState.TotalFailed);
            if (completeState.TotalFailed > 0)
                failedMessage.WrapConsoleMessageWithColor(ConsoleColor.Red, false);
            else
                writer.Write(failedMessage);

            var ignoredMessage = "Ignored {0}".FormatWith(completeState.TotalIgnored);
            if (completeState.TotalIgnored > 0)
                ignoredMessage.WrapConsoleMessageWithColor(ConsoleColor.Yellow, false);

            writer.WriteLine("");
        }
    }
}
