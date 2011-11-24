using System;
using System.Collections.Generic;
using System.Linq;
using StatLight.Core.Properties;
using writer = System.Console;

namespace StatLight.Core.Reporting.Providers.Console
{
    public static class ConsoleTestCompleteMessage
    {
        public static void WriteOutCompletionStatement(TestReport completeState, DateTime startOfRunTime)
        {
            if (completeState == null) throw new ArgumentNullException("completeState");
            WriteOutSummary(
                completeState.TotalResults,
                completeState.TotalPassed,
                completeState.TotalFailed,
                completeState.TotalIgnored,
                startOfRunTime);
        }

        private static void WriteOutSummary(int totalResults, int totalPassed, int totalFailed, int totalIgnored, DateTime startOfRunTime)
        {
            writer.Write("{1}{1}-- Completed Test Run at: {0}. Total Run Time: {2}{1}{1}"
                .FormatWith(DateTime.Now, Environment.NewLine, DateTime.Now.Subtract(startOfRunTime)));

            writer.Write("Test run results: Total {0}, ", totalResults);

            var successfulMessage = "Successful {0}, ".FormatWith(totalPassed);
            if (totalPassed > 0)
                successfulMessage.WrapConsoleMessageWithColor(Settings.Default.ConsoleColorSuccess, false);
            else
                writer.Write(successfulMessage);

            var failedMessage = "Failed {0}, ".FormatWith(totalFailed);
            if (totalFailed > 0)
                failedMessage.WrapConsoleMessageWithColor(Settings.Default.ConsoleColorError, false);
            else
                writer.Write(failedMessage);

            var ignoredMessage = "Ignored {0}".FormatWith(totalIgnored);
            if (totalIgnored > 0)
                ignoredMessage.WrapConsoleMessageWithColor(Settings.Default.ConsoleColorWarning, false);

            writer.WriteLine("");
        }

        public static void PrintFinalTestSummary(IEnumerable<TestReport> testReports, DateTime startOfRunTime)
        {
            if (testReports == null)
                throw new ArgumentNullException("testReports");

            int totalResults = 0;
            int totalPassed = 0;
            int totalFailed = 0;
            int totalIgnored = 0;

            foreach (var testReport in testReports)
            {
                totalResults += testReport.TotalResults;
                totalPassed += testReport.TotalPassed;
                totalFailed += testReport.TotalFailed;
                totalIgnored += testReport.TotalIgnored;

                if (testReport.Failures.Any())
                {
                    "********************************************"
                        .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);
                    ("Test Result Summary for: " + testReport.XapPath)
                        .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);

                    testReport.Failures.Each(ConsoleResultHandler.WriteOutError);

                    "********************************************"
                        .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);
                }
            }

            WriteOutSummary(
                totalResults,
                totalPassed,
                totalFailed,
                totalIgnored,
                startOfRunTime);
        }
    }
}
