
namespace StatLight.Core.Reporting.Providers.NUnit
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using StatLight.Core.Events;
    using StatLight.Core.Properties;
    using StatLight.Core.Reporting.Providers.Xml;


    public class NUnitXmlReport : IXmlReport
    {
        private readonly TestReportCollection _report;

        public NUnitXmlReport(TestReportCollection report)
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


        public string GetXmlReport()
        {
            var results = _report;

            return new XElement("test-results",
                new XAttribute("name", ""),
                new XAttribute("total", results.TotalResults),
                new XAttribute("not-run", results.TotalIgnored),
                new XAttribute("failures", results.TotalFailed),
                new XAttribute("date", results.DateTimeRunCompleted.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)),
                new XAttribute("time", results.DateTimeRunCompleted.ToString("HH:mm:ss", CultureInfo.InvariantCulture)),
                new XElement("test-suite",
                    new XElement("results",
                        from report in results
                        select GetReport(report)
                        )
                )).ToString();
        }

        private static XElement GetReport(TestReport report)
        {
            return new XElement("test-suite",
                                new XAttribute("type", "TestFixture"),
                                new XAttribute("name", Path.GetFileName(report.XapPath)),
                                new XAttribute("executed", "True"),
                                new XAttribute("result", report.TotalFailed > 0 ? "Failure" : "Success"),
                                new XAttribute("success", report.TotalFailed > 0 ? "False" : "True"),
                                new XElement("results",
                                                from r in report.TestResults
                                                select CreateTestCaseElement(r))
                );

        }

        private static XElement CreateTestCaseElement(TestCaseResultServerEvent resultServerEvent)
        {
            var element = new XElement("test-case",
                                new XAttribute("name", resultServerEvent.FullMethodName()),
                                new XAttribute("executed", resultServerEvent.ResultType == ResultType.Ignored ? "False" : "True"),
                                new XAttribute("time", resultServerEvent.TimeToComplete.ToString(@"hh\:mm\:ss\.ffff", CultureInfo.InvariantCulture)),
                                new XAttribute("result", resultServerEvent.ResultType == ResultType.Ignored ? "Ignored" : (resultServerEvent.ResultType == ResultType.Failed || resultServerEvent.ResultType == ResultType.SystemGeneratedFailure) ? "Failure" : "Success"));

            if (resultServerEvent.ResultType != ResultType.Ignored)
                element.Add(new XAttribute("success", resultServerEvent.ResultType == ResultType.Passed ? "True" : "False"));

            if (resultServerEvent.ResultType == ResultType.Failed || resultServerEvent.ResultType == ResultType.SystemGeneratedFailure)
                element.Add(new XElement("failure",
                    new XElement("message", GetErrorMessage(resultServerEvent)),
                    new XElement("stack-trace", new XCData(GetErrorStackTrace(resultServerEvent)))));

            return element;
        }

        private static string GetErrorStackTrace(TestCaseResultServerEvent testCaseResultServerEvent)
        {
            if (testCaseResultServerEvent.ExceptionInfo != null)
                return testCaseResultServerEvent.ExceptionInfo.StackTrace ?? "";
            return testCaseResultServerEvent.OtherInfo ?? "";
        }

        private static string GetErrorMessage(TestCaseResultServerEvent r)
        {
            if (r.ExceptionInfo != null)
                return r.ExceptionInfo.FullMessage ?? "";
            return r.OtherInfo ?? "";
        }

        public static bool ValidateSchema(string pathToXmlFileToValidate, out IList<string> validationErrors)
        {
            return XmlSchemaValidatorHelper.ValidateSchema(pathToXmlFileToValidate, Resources.NUnitXmlResults, out validationErrors);
        }
    }
}
