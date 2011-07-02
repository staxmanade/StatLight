
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

        private static XElement CreateTestCaseElement(TestCaseResult r)
        {
            XElement element = new XElement("test-case",
                                        new XAttribute("name", r.FullMethodName()),
                                        new XAttribute("executed", r.ResultType == ResultType.Ignored ? "False" : "True"),
                                        new XAttribute("time", r.TimeToComplete.ToString(@"hh\:mm\:ss\.ffff", CultureInfo.InvariantCulture)),
                                        new XAttribute("result", r.ResultType == ResultType.Ignored ? "Ignored" : (r.ResultType == ResultType.Failed || r.ResultType == ResultType.SystemGeneratedFailure) ? "Failure" : "Success"));
            if (r.ResultType != ResultType.Ignored)
                element.Add(new XAttribute("success", r.ResultType == ResultType.Passed ? "True" : "False"));

            if (r.ResultType == ResultType.Failed || r.ResultType == ResultType.SystemGeneratedFailure)
                element.Add(new XElement("failure",
                    new XElement("message", GetErrorMessage(r)),
                    new XElement("stack-trace", new XCData(GetErrorStackTrace(r)))));

            return element;
        }

        private static string GetErrorStackTrace(TestCaseResult testCaseResult)
        {
            if (testCaseResult.ExceptionInfo != null)
                return testCaseResult.ExceptionInfo.StackTrace ?? "";
            return testCaseResult.OtherInfo ?? "";
        }

        private static string GetErrorMessage(TestCaseResult r)
        {
            if (r.ExceptionInfo != null)
                return r.ExceptionInfo.FullMessage ?? "";
            return r.OtherInfo ?? "";
        }


        //public string GetXmlReport()
        //{
        //    /*
        //     name="C:\Users\jasonj\Documents\Visual Studio 2010\Projects\ClassLibrary1\ClassLibrary1\bin\Debug\ClassLibrary1.nunit" 
        //     * total="4"
        //     * errors="2"
        //     * failures="0" 
        //     * not-run="0" 
        //     * inconclusive="0" 
        //     * ignored="0" 
        //     * skipped="0"
        //     * invalid="0"
        //     * date="2011-07-02" 
        //     * time="10:45:44"
        //     */
        //    var root = 
        //            new XElement("test-results"
        //                , new XAttribute("name", "")
        //                , new XAttribute("total", _report.TotalResults)
        //                , new XAttribute("failures", _report.TotalFailed)
        //                , new XAttribute("ignored", _report.TotalIgnored)
        //                , new XAttribute("date", _report.DateTimeRunCompleted.ToString("yyyy-MM-dd", CultureInfo.CurrentCulture))
        //                , new XAttribute("time", _report.DateTimeRunCompleted.ToString("HH:mm:ss", CultureInfo.CurrentCulture))
        //                , GetTestsRuns(_report)
        //            );
        //    return root.ToString();
        //}

        //private static List<XElement> GetTestsRuns(IEnumerable<TestReport> report)
        //{
        //    return report.Select(item =>
        //            new XElement("tests",
        //                new XAttribute("xapFileName", item.XapPath),
        //                item.TestResults.Select(GetResult))).ToList();
        //}

        //private static XElement GetResult(TestCaseResult result)
        //{
        //    Func<TestCaseResult, string> formatName =
        //        resultX => resultX.FullMethodName();

        //    XElement otherInfoElement = null;
        //    if (!string.IsNullOrEmpty(result.OtherInfo))
        //    {
        //        otherInfoElement = new XElement("otherInfo", result.OtherInfo);
        //    }

        //    XElement exceptionInfoElement = null;
        //    if (result.ExceptionInfo != null)
        //    {
        //        exceptionInfoElement = FormatExceptionInfoElement(result.ExceptionInfo);
        //    }

        //    return new XElement("test",
        //                new XAttribute("name", formatName(result)),
        //                new XAttribute("resulttype", result.ResultType),
        //                new XAttribute("timeToComplete", result.TimeToComplete.ToString()),
        //                exceptionInfoElement,
        //                otherInfoElement
        //                );
        //}

        //private static XElement FormatExceptionInfoElement(ExceptionInfo exceptionInfo)
        //{
        //    if (exceptionInfo == null)
        //        return null;

        //    return FormatExceptionInfoElement(exceptionInfo, false);
        //}

        //private static XElement FormatExceptionInfoElement(ExceptionInfo exceptionInfo, bool isInnerException)
        //{
        //    if (exceptionInfo == null)
        //        return null;

        //    string elementName = "exceptionInfo";

        //    if (isInnerException)
        //        elementName = "innerExceptionInfo";

        //    return new XElement(elementName
        //                    , new XElement("message", exceptionInfo.Message)
        //                    , new XElement("stackTrace", exceptionInfo.StackTrace)
        //                    , FormatExceptionInfoElement(exceptionInfo.InnerException, true)
        //                    );
        //}


        public static bool ValidateSchema(string pathToXmlFileToValidate, out IList<string> validationErrors)
        {
            return XmlSchemaValidatorHelper.ValidateSchema(pathToXmlFileToValidate, Resources.NUnitXmlResults, out validationErrors);
        }
    }
}
