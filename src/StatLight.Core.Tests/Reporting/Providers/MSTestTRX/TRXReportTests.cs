using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
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
            string tempFileName = @"C:\Users\jasonj\AppData\Local\Temp\tmp8699.trx"; //Path.GetTempFileName();
            try
            {
                var report = new TestReportCollection();
                var testReport = new TestReport(@"C:\Test.xap");
                testReport.AddResult(TestCaseResultFactory.CreatePassed());
                //testReport.AddResult(TestCaseResultFactory.CreateFailed());
                //testReport.AddResult(TestCaseResultFactory.CreateIgnored());

                report.Add(testReport);
                var trxReport = new TRXReport(report);
                trxReport.WriteXmlReport(tempFileName);

                //File.ReadAllText(tempFileName).Trace();

                var xmlSchema = Resources.vstst;
                IList<string> validationErrors;
                XmlSchemaValidatorHelper.ValidateSchema(tempFileName, xmlSchema, out validationErrors);

                tempFileName.Trace();

                foreach (var validationError in validationErrors)
                {
                    validationError.Trace();
                }

                if (validationErrors.Any())
                {
                    Assert.Fail("Validation Errors:{0}{1}".FormatWith(Environment.NewLine, string.Join(Environment.NewLine, validationErrors)));
                }
            }
            catch (Exception)
            {
                //if(File.Exists(tempFileName))
                //    File.Delete(tempFileName);
                throw;
            }
        }

        [Test]
        public void Should_spit_out_valid_MSTest_trx()
        {
            var report = new TestReportCollection();
            var testReport = new TestReport(@"C:\Test.xap");
            testReport.AddResult(TestCaseResultFactory.CreatePassed());
            testReport.AddResult(TestCaseResultFactory.CreateFailed());
            //testReport.AddResult(TestCaseResultFactory.CreateIgnored());
            report.Add(testReport);

            var testSettings = new TestSettings();
            var trxReport = new TRXReport(report, new MockGuidSequenceGenerator(), testSettings);
            var output = trxReport.GetXmlReport();

            var memoryStream = new MemoryStream();
            using (var writer = new StreamWriter(memoryStream))
            {
                var xml = output;
                xml.Save(writer);
                writer.Close();
            }

            string fileData = memoryStream.ToArray().ToStringFromByteArray();

            fileData.ShouldEqual(Resources.SampleTRX_GeneratedFromRealTest);

            //fileData.Trace();
        }

    }


    public class MockGuidSequenceGenerator : IGuidSequenceGenerator
    {
        private List<Guid> _guids = new List<Guid>();
        private int instance = 0;

        public MockGuidSequenceGenerator()
        {
            _guids.AddRange(
            new[] {
                new Guid("e83f94ca-b0fe-41dd-af9e-9f7adcefe7a0"), //TestRun
                new Guid("184c7c57-d71a-480f-9de6-18ff3b15a7ff"), //TestSettings
                new Guid("becf20cb-74e3-3a4f-2d5f-8311683824ed"), // <UnitTest name="MethodPassed"
                new Guid("06e3924c-14f5-46e7-b640-77e77598e6c0"), //       <Execution id
                new Guid("264ee2b2-3b8f-645c-d9ff-739a31f9c1e6"), // <UnitTest name="MethodFailed"
                new Guid("e9d88bbd-1814-445e-84fb-876d083d02d9"), //       <Execution id
                new Guid("8c84fa94-04c1-424b-9868-57a2d4851a1d"), // TestList 1
                new Guid("8c84fa94-04c1-424b-9868-57a2d4851a1d"),
                new Guid("8c84fa94-04c1-424b-9868-57a2d4851a1d"),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
                new Guid(),
            });
        }

        public Guid Next()
        {
            if (instance+1 == _guids.Count)
                throw new Exception("No Guid configured for index # {0}".FormatWith(instance));
            return _guids[instance++];
        }
    }
}