using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;
using StatLight.Client.Harness.Events;

namespace StatLight.Core.Reporting.Providers.Xml
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using StatLight.Core.Common;
    using StatLight.Core.Properties;
    using System.Text;

    public class XmlReport
    {
        private readonly TestReport _report;
        private readonly string _testXapFileName;

        public XmlReport(TestReport report, string testXapFileName)
        {
            _report = report;
            _testXapFileName = testXapFileName;
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
            Debug.Assert(_report != null);

            IEnumerable<XElement> testItems = (from x in _report.TestResults
                                               select GetResult(x));

            var root =
                    new XElement("StatLightTestResults"
                        , new XAttribute("xapFileName", _testXapFileName)
                        , new XAttribute("total", _report.TotalResults)
                        , new XAttribute("ignored", _report.TotalIgnored)
                        , new XAttribute("failed", _report.TotalFailed)
                        , new XAttribute("dateRun", _report.DateTimeRunCompleted.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture))

                        , new XElement("tests", testItems)
                    );
            return root.ToString();
        }

        private static XElement GetResult(TestCaseResult result)
        {
            Func<TestCaseResult, string> formatName =
                resultX => "{0}.{1}.{2}".FormatWith(resultX.NamespaceName, resultX.ClassName, resultX.MethodName);

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
            validationErrors = new List<string>();

            var currentValidationErrors = new List<string>();

            string xsdSchemaString = Resources.XmlReportSchema;

            var stringReader = new StringReader(xsdSchemaString);
            var xmlReader = XmlReader.Create(stringReader);
            var schema = XmlSchema.Read(xmlReader, null);
            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(schema);

            var settings = new XmlReaderSettings();
            settings.ValidationEventHandler += (sender, e) => currentValidationErrors.Add(e.Message);
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schemaSet;

            var reader = XmlReader.Create(pathToXmlFileToValidate, settings);

            while (reader.Read())
            {
            }

            if (currentValidationErrors.Count > 0)
            {
                validationErrors = currentValidationErrors;
                return false;
            }
            return true;
        }
    }
}
