
namespace StatLight.Core.Reporting.Providers.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Events;
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
                var xml = GetXmlReport();
                xml.Save(writer);
                //writer.Write(xml.ToString(options:SaveOptions.None));
                writer.Close();
            }
        }

        public XDocument GetXmlReport()
        {
            var root = new XDocument(
                new XElement("StatLightTestResults"
                    , new XAttribute("total", _report.TotalResults)
                    , new XAttribute("ignored", _report.TotalIgnored)
                    , new XAttribute("failed", _report.TotalFailed)
                    , new XAttribute("dateRun", _report.DateTimeRunCompleted.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture))

                    , GetTestsRuns(_report)
                ))
                {
                    Declaration = new XDeclaration("1.0", "", "")
                };
            return root;
        }

        private static List<XElement> GetTestsRuns(IEnumerable<TestReport> report)
        {
            return report.Select(item =>
                    new XElement("tests",
                        new XAttribute("xapFileName", item.XapPath),
                        item.TestResults.Select(GetResult))).ToList();
        }

        private static XElement GetResult(TestCaseResultServerEvent resultServerEvent)
        {
            Func<TestCaseResultServerEvent, string> formatName =
                resultX => resultX.FullMethodName();

            XElement otherInfoElement = null;
            if (!string.IsNullOrEmpty(resultServerEvent.OtherInfo))
            {
                otherInfoElement = new XElement("otherInfo", resultServerEvent.OtherInfo);
            }

            XElement exceptionInfoElement = null;
            if (resultServerEvent.ExceptionInfo != null)
            {
                exceptionInfoElement = FormatExceptionInfoElement(resultServerEvent.ExceptionInfo);
            }

            XElement metaData = null;
            if (resultServerEvent.Metadata.Any())
            {
                metaData = new XElement("metaDataItems", from md in resultServerEvent.Metadata
                                                         select new XElement("metaData", 
                                                                    new XAttribute("classification", md.Classification), 
                                                                    new XAttribute("name", md.Name), 
                                                                    md.Value));
            }

            return new XElement("test",
                        new XAttribute("name", formatName(resultServerEvent)),
                        new XAttribute("resulttype", resultServerEvent.ResultType),
                        new XAttribute("timeToComplete", resultServerEvent.TimeToComplete.ToString()),
                        exceptionInfoElement,
                        otherInfoElement,
                        metaData
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
