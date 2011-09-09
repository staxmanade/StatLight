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
        private readonly IGuidSequenceGenerator _guidSequenceGenerator;
        private Guid TestListId;

        public TRXReport(TestReportCollection report)
            : this(report, new GuidSequenceGenerator())
        {
        }

        public TRXReport(TestReportCollection report, IGuidSequenceGenerator guidSequenceGenerator)
        {
            if (report == null)
                throw new ArgumentNullException("report");

            _report = report;
            _guidSequenceGenerator = guidSequenceGenerator;
            TestListId = _guidSequenceGenerator.Next();
        }


        public void WriteXmlReport(string outputFilePath)
        {
            using (var writer = new StreamWriter(outputFilePath))
            {
                var xml = GetXmlReport();
                xml.Save(writer);
                writer.Close();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public XDocument GetXmlReport()
        {
            var results = _report;

            var ns = XNamespace.Get(@"http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "")
                , new XElement(ns + "TestRun"
                    , new XAttribute("id", _guidSequenceGenerator.Next().ToString())
                    , new XAttribute("name", "StatLight TestRun")
                //, GetTestSettingsNode()
                //,new XElement(ns +"TestRunConfiguration", new XAttribute("name", "StatLight Test Run"), new XAttribute("id", _guidSequenceGenerator.Next().ToString()))
                //,GetResultSummary(results)
                //,GetTestDefinitions(results)
                    , new XElement(ns + "TestEntries",
                        new XElement(ns + "TestEntry"
                            , new XAttribute("testId", "264ee2b2-3b8f-645c-d9ff-739a31f9c1e6")
                            , new XAttribute("executionId", "e9d88bbd-1814-445e-84fb-876d083d02d9")
                            , new XAttribute("testListId", TestListId)
                            )
                        )
                    , GetResults(results, ns)
                    )
                );

            //doc.Root.SetAttributeValue("xmlns", @"http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            return doc;
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
                        new XAttribute("id", _guidSequenceGenerator.Next().ToString()),
                        new XAttribute("name", test.FullMethodName()))
                );
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.TimeSpan.ToString(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private XElement GetResults(IEnumerable<TestReport> results, XNamespace ns)
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

            return new XElement(ns + "Results",
                from report in results
                from test in report.TestResults
                select new XElement(ns + "UnitTestResult"
                    //, new XAttribute("executionId", "e9d88bbd-1814-445e-84fb-876d083d02d9")
                        , new XAttribute("testId", _guidSequenceGenerator.Next().ToString())
                        , new XAttribute("testName", test.FullMethodName())
                        , new XAttribute("testType", "SilverlightTest")
                        , new XAttribute("testListId", TestListId)
                        , new XAttribute("computerName", Environment.MachineName)
                    //, new XAttribute("duration", test.TimeToComplete.ToString()) //TODO: format the milliseconds (EX: duration="00:00:00.0193753")
                    //, new XAttribute("startTime", test.Started) //TODO: format like startTime="2011-08-23T12:34:52.4182565-07:00"
                    //, new XAttribute("endTime", test.Finished ?? new DateTime()) //TODO: format like endTime="2011-08-23T12:34:52.7462753-07:00"
                    //, new XAttribute("outcome", getResult(test.ResultType))
                        )
                //The required attribute 'testType' is missing.
                //The required attribute 'testId' is missing.
                //The required attribute 'testListId' is missing.
                //The required attribute 'computerName' is missing.
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

    public class GuidSequenceGenerator : IGuidSequenceGenerator
    {
        public Guid Next()
        {
            return Guid.NewGuid();
        }
    }

    public interface IGuidSequenceGenerator
    {
        Guid Next();
    }

}