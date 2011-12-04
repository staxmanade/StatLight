using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using StatLight.Core.Properties;
using writer = System.Console;

namespace StatLight.Core.Reporting.Providers.Console
{
    public static class ConsoleTestCompleteMessage
    {
        public static void WriteOutCompletionStatement(TestReport completeState, TimeSpan totalTimeOfRun)
        {
            if (completeState == null) throw new ArgumentNullException("completeState");
            WriteOutSummary(
                completeState.TotalResults,
                completeState.TotalPassed,
                completeState.TotalFailed,
                completeState.TotalIgnored,
                totalTimeOfRun,
                Path.GetFileName(completeState.XapPath));
        }

        private static void WriteOutSummary(int totalResults, int totalPassed, int totalFailed, int totalIgnored, TimeSpan startOfRunTime, string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                "*************** Summary ********************"
                    .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);
            }
            else
            {
                "*************** Summary for : {0}"
                    .FormatWith(fileName)
                    .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);
            }
            "********************************************"
                .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);

            var settings = Settings.Default;

            Action<string, string, ConsoleColor> w = (name, value, color) => 
                "{0,-15}: {1}"
                    .FormatWith(name, value)
                    .WrapConsoleMessageWithColor(color, true);

            w("Total", totalResults.ToString(CultureInfo.CurrentCulture), settings.ConsoleColorInformation);
            w("Successful", totalPassed.ToString(CultureInfo.CurrentCulture), totalPassed > 0 ? Settings.Default.ConsoleColorSuccess : Settings.Default.ConsoleColorInformation);
            w("Failed", totalFailed.ToString(CultureInfo.CurrentCulture), totalFailed > 0 ? Settings.Default.ConsoleColorError : Settings.Default.ConsoleColorInformation);
            w("Ignored", totalIgnored.ToString(CultureInfo.CurrentCulture), totalIgnored > 0 ? Settings.Default.ConsoleColorWarning : Settings.Default.ConsoleColorInformation);
            w("Completion End", DateTime.Now.ToString(CultureInfo.CurrentCulture), settings.ConsoleColorInformation);
            w("Duration", startOfRunTime.ToString(), settings.ConsoleColorInformation);
            "********************************************"
                .WrapConsoleMessageWithColor(Settings.Default.ConsoleColorInformation, true);
        }

        public static void PrintFinalTestSummary(IEnumerable<TestReport> testReports, TimeSpan totalTimeOfRun)
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
                    ("Error Summary for: " + testReport.XapPath)
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
                totalTimeOfRun,
                "");
        }
    }
}
