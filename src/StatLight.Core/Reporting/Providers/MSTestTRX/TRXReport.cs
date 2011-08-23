using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using StatLight.Core.Events;

namespace StatLight.Core.Reporting.Providers.MSTestTRX
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TRX")]
    public class TRXReport : IXmlReport
    {
        private readonly TestReportCollection _report;

        public TRXReport(TestReportCollection report)
        {
            if (report == null)
                throw new ArgumentNullException("report");

            _report = report;
        }


        public void WriteXmlReport(string outputFilePath)
        {
            using (var writer = new StreamWriter(outputFilePath))
            {
                writer.Write(GetXmlReport());
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public string GetXmlReport()
        {
            var results = _report;

            return new XElement("TestRun",
                GetTestSettingsNode(),
                GetResultSummary(results),
                GetTestDefinitions(results),
                GetResults(results)
                ).ToString();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private XElement GetTestSettingsNode()
        {
            return new XElement("TestSettings",
                new XElement("Description", "These are default test settings for a local test run."),
                //new XElement("Deployment", new XAttribute("enabled", false)),
                new XElement("Execution",
                    new XElement("TestTypeSpecific"),
                    new XElement("AgentRule", new XAttribute("name", "Execution Agents"))
                    )
                );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private XElement GetResultSummary(TestReportCollection results)
        {
            return new XElement("ResultSummary",
                    new XAttribute("outcome", (results.FinalResult == RunCompletedState.Failure ? "Failed" : "Completed")),
                    new XElement("Counters",
                        new XAttribute("total", results.TotalResults),
                        new XAttribute("executed", results.TotalResults),
                        new XAttribute("passed", results.TotalPassed),
                        new XAttribute("error", 0),
                        new XAttribute("failed", results.TotalFailed),
                        new XAttribute("timeout", 0),
                        new XAttribute("aborted", 0),
                        new XAttribute("inconclusive", 0),
                        new XAttribute("passedButRunAborted", 0),
                        new XAttribute("notRunnable", 0),
                        new XAttribute("notExecuted", 0),
                        new XAttribute("disconnected", 0),
                        new XAttribute("warning", 0),
                        new XAttribute("completed", 0),
                        new XAttribute("inProgress", 0),
                        new XAttribute("pending", 0)
                        )
                );
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private XElement GetTestDefinitions(IEnumerable<TestReport> results)
        {
            return new XElement("TestDefinitions",
                    from report in results
                    from test in report.TestResults
                    select new XElement("UnitTest",
                        new XAttribute("name", test.FullMethodName()))
                );
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.TimeSpan.ToString(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private XElement GetResults(IEnumerable<TestReport> results)
        {
            Func<ResultType, string> getResult = r =>
            {
                if (r == ResultType.Failed)
                    return "Failed";
                if (r == ResultType.SystemGeneratedFailure)
                    return "Failed";
                if (r == ResultType.Passed)
                    return "Passed";

                if (r == ResultType.Ignored)
                    return "Ignored";

                throw new NotImplementedException("Unknown result type [{0}]".FormatWith(r.ToString()));
            };

            return new XElement("Results",
                from report in results
                from test in report.TestResults
                select new XElement("UnitTestResult"
                        ,new XAttribute("testName", test.FullMethodName())
                        ,new XAttribute("duration", test.TimeToComplete) //TODO: format the milliseconds (EX: duration="00:00:00.0193753")
                        ,new XAttribute("startTime", test.Started) //TODO: format like startTime="2011-08-23T12:34:52.4182565-07:00"
                        ,new XAttribute("endTime", test.Finished ?? new DateTime()) //TODO: format like endTime="2011-08-23T12:34:52.7462753-07:00"
                        ,new XAttribute("outcome", getResult(test.ResultType))
                        )
                );
        }

        //private static string GetErrorStackTrace(TestCaseResult testCaseResult)
        //{
        //    if (testCaseResult.ExceptionInfo != null)
        //        return testCaseResult.ExceptionInfo.StackTrace ?? "";
        //    return testCaseResult.OtherInfo ?? "";
        //}

        //private static string GetErrorMessage(TestCaseResult r)
        //{
        //    if (r.ExceptionInfo != null)
        //        return r.ExceptionInfo.FullMessage ?? "";
        //    return r.OtherInfo ?? "";
        //}
    }
}