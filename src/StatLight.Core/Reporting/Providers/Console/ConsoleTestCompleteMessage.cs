using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using writer = System.Console;

namespace StatLight.Core.Reporting.Providers.Console
{
    public static class ConsoleTestCompleteMessage
    {
        public static void WriteOutCompletionStatement(TestReport completeState)
        {
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
