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
            //testReport.AddResult(TestCaseResultFactory.CreateFailed());
            //testReport.AddResult(TestCaseResultFactory.CreateIgnored());

            var trxReport = new TRXReport(report, new MockGuidSequenceGenerator());
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

        [Test]
        public void GuidSequenceGenerator_should_generate_new_guids()
        {
            var guidSequenceGenerator = new GuidSequenceGenerator();
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
                new Guid("e83f94ca-b0fe-41dd-af9e-9f7adcefe7a0"),
                new Guid("e83f94ca-b0fe-41dd-af9e-9f7adcefe7a0"),
                new Guid("c0e5a377-3190-4f62-be7a-19cda521dfcd"),
                new Guid("c05f46c3-ac77-472a-a98a-cf2d5fcb86da"),
                new Guid("4b5f994c-3a52-4fa6-ada8-ed5c21848f2d"),
                new Guid("4f184d94-236d-4a92-9976-163d56434c70"),
                new Guid("dbfa5c76-83fc-4bb3-95c8-ac0455525471"),
                new Guid("80b673ee-414f-4e71-a043-fc62e46cb518"),
                new Guid("a48c8a5e-1f6e-4955-8061-b1b9d00b78dc"),
                new Guid("7b9e93ac-0bfc-4494-b044-980f1add530c"), 
            });
        }

        public Guid Next()
        {
            return _guids[instance++];
        }
    }
}