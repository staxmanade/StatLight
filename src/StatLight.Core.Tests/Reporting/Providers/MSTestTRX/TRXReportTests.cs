using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using StatLight.Core.Reporting;
using StatLight.Core.Reporting.Providers.MSTestTRX;
using StatLight.Core.Reporting.Providers.Xml;

namespace StatLight.Core.Tests.Reporting.Providers.MSTestTRX
{
    public class TRXReportTests : FixtureBase
    {
        [Test]
        public void Should_conform_to_xml_schema()
        {
            string tempFileName = Path.GetTempFileName();
            try
            {
                var report = new TestReportCollection();
                var testReport = new TestReport(@"C:\Test.xap");
                testReport.AddResult(TestCaseResultFactory.CreatePassed());
                testReport.AddResult(TestCaseResultFactory.CreateFailed());
                testReport.AddResult(TestCaseResultFactory.CreateIgnored());

                report.Add(testReport);
                var trxReport = new TRXReport(report);
                trxReport.WriteXmlReport(tempFileName);

                File.ReadAllText(tempFileName).Trace();

                string xmlSchemaPath = @"C:\Program Files (x86)\Microsoft Visual Studio 10.0\Xml\Schemas\vstst.xsd";
                IList<string> validationErrors = new List<string>();
                XmlSchemaValidatorHelper.ValidateSchema(tempFileName, File.ReadAllText(xmlSchemaPath), out validationErrors);

                foreach (var validationError in validationErrors)
                {
                    validationError.Trace();
                }

                validationErrors.Count.ShouldEqual(0);
            }
            catch (Exception)
            {
                if(File.Exists(tempFileName))
                    File.Delete(tempFileName);
                throw;
            }
        }
    }
}