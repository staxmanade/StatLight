namespace StatLight.Core.Reporting.Providers.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using StatLight.Client.Harness.Events;
    using StatLight.Core.Properties;

    public class XmlReport : IXmlReport
    {
        private readonly TestReportCollection _report;

        public XmlReport(TestReportCollection report)
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
                writer.Close();
            }
        }

        public string GetXmlReport()
        {
            var root =
                    new XElement("StatLightTestResults"
                        , new XAttribute("total", _report.TotalResults)
                        , new XAttribute("ignored", _report.TotalIgnored)
                        , new XAttribute("failed", _report.TotalFailed)
                        , new XAttribute("dateRun", _report.DateTimeRunCompleted.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture))

                        , GetTestsRuns(_report)
                    );
            return root.ToString();
        }

        private static List<XElement> GetTestsRuns(IEnumerable<TestReport> report)
        {
            return report.Select(item => 
                    new XElement("tests", 
                        new XAttribute("xapFileName", item.XapPath), 
                        item.TestResults.Select(GetResult))).ToList();
        }

        private static XElement GetResult(TestCaseResult result)
        {
            Func<TestCaseResult, string> formatName = 
                resultX => resultX.FullMethodName();

            XElement otherInfoElement = null;
            if (!string.IsNullOrEmpty(result.OtherInfo))
            {
                otherInfoElement = new XElement("otherInfo", result.OtherInfo);
            }

            XElement exceptionInfoElement = null;
            if (result.ExceptionInfo != null)
            {
                exceptionInfoElement = FormatExceptionInfoElement(result.ExceptionInfo);
            }

            return new XElement("test",
                        new XAttribute("name", formatName(result)),
                        new XAttribute("resulttype", result.ResultType),
                        new XAttribute("timeToComplete", result.TimeToComplete.ToString()),
                        exceptionInfoElement,
                        otherInfoElement
                        );
        }

        private static XElement FormatExceptionInfoElement(ExceptionInfo exceptionInfo)
        {
            if (exceptionInfo == null)
                return null;

            return FormatExceptionInfoElement(exceptionInfo, false);
        }

        private static XElement FormatExceptionInfoElement(ExceptionInfo exceptionInfo, bool isInnerException)
        {
            if (exceptionInfo == null)
                return null;

            string elementName = "exceptionInfo";

            if (isInnerException)
                elementName = "innerExceptionInfo";

            return new XElement(elementName
                            , new XElement("message", exceptionInfo.Message)
                            , new XElement("stackTrace", exceptionInfo.StackTrace)
                            , FormatExceptionInfoElement(exceptionInfo.InnerException, true)
                            );
        }


        public static bool ValidateSchema(string pathToXmlFileToValidate, out IList<string> validationErrors)
        {
            return XmlSchemaValidatorHelper.ValidateSchema(pathToXmlFileToValidate, Resources.XmlReportSchema, out validationErrors);
        }
    }
}
