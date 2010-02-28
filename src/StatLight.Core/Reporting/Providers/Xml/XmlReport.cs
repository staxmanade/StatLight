using System.Diagnostics;
using System.Xml;
using System.Xml.Schema;

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
                                               where x is TestCaseResult
                                               select GetResult((TestCaseResult)x));

            var root =
                    new XElement("StatLightTestResults"
                        , new XAttribute("xapFileName", _testXapFileName)
                        , new XAttribute("total", _report.TotalResults)
                        , new XAttribute("ignored", _report.TotalIgnored)
                        , new XAttribute("failed", _report.TotalFailed)
                        , new XAttribute("dateRun", _report.DateTimeRunCompleted.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.CurrentCulture))

                        , new XElement("tests"
                            , testItems
                            )

                ////(from x in _report.OtherMessages
                //// select GetOtherMessage(x))
                //        )
                    );
            return root.ToString();
        }

        private static XElement GetResult(TestCaseResult result)
        {
            Func<TestCaseResult, string> formatName =
                resultX => "{0}.{1}".FormatWith(resultX.ClassName, resultX.MethodName);

            Func<ResultType, TestCaseResult, IEnumerable<XAttribute>> getCommonAttributes = (resultType, resultX) => new[]
				{
					new XAttribute("name", formatName(resultX)),
					new XAttribute("resulttype", resultType),
					new XAttribute("timeToComplete", resultX.TimeToComplete.ToString()),
				};

            switch (result.ResultType)
            {
                case ResultType.Passed:
                    return GetTestCaseElement(
                                getCommonAttributes(result.ResultType, result)
                            );
                case ResultType.Failed:
                    return GetTestCaseElement(
                                getCommonAttributes(result.ResultType, result),
                                new XElement("failureMessage", new XCData(result.ExceptionInfo.FullMessage))
                                );
                case ResultType.Ignored:
                    return GetTestCaseElement(
                                getCommonAttributes(result.ResultType, result)
                                );
                case ResultType.SystemGeneratedFailure:
                    throw new NotImplementedException();
                default:
                    throw new StatLightException("Unknown result type {0}".FormatWith(result.ResultType.ToString()));
            }
        }

        private static XElement GetTestCaseElement(params object[] attributes)
        {
            return new XElement("test", attributes);
        }

        public static bool ValidateSchema(string filxeToValidate, out IList<string> validationErrors)
        {
            validationErrors = new List<string>();

            var currentValidationErrors = new List<string>();

            string xsdSchemaString = Properties.Resources.XmlReportSchema;

            var stringReader = new StringReader(xsdSchemaString);
            var xmlReader = XmlReader.Create(stringReader);
            var schema = XmlSchema.Read(xmlReader, null);
            var schemaSet = new XmlSchemaSet();
            schemaSet.Add(schema);

            var settings = new XmlReaderSettings();
            settings.ValidationEventHandler += (sender, e) => currentValidationErrors.Add(e.Message);
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = schemaSet;

            var reader = XmlReader.Create(filxeToValidate, settings);

            while (reader.Read())
            {
            }

            System.Threading.Thread.Sleep(1000);

            if (currentValidationErrors.Count > 0)
            {
                validationErrors = currentValidationErrors;
                return false;
            }
            return true;
        }
    }
}
