using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using NUnit.Framework;
using StatLight.Core.Events;
using StatLight.Core.Reporting;
using StatLight.Core.Reporting.Providers.MSTestTRX;
using StatLight.Core.Reporting.Providers.Xml;

namespace StatLight.Core.Tests.Reporting.Providers.MSTestTRX
{
    [Ignore]
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

            report
                .AllTests()
                .Where(w => w.ResultType == ResultType.Failed)
                .Each(x => x.ExceptionInfo.StackTrace = "Some message that will be a stacktrace");

            var testSettings = new TestSettings();
            testSettings.ComputerName = "UserName-LT3";
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
            string expectedFileData = Resources.SampleTRX_GeneratedFromRealTest;

            FixupRegEx("duration=\"00:00:00.0000000\"", ref expectedFileData, ref fileData,
                @"duration=\""[0-9][0-9]:[0-9][0-9]:[0-9][0-9]\.[0-9][0-9][0-9][0-9][0-9][0-9][0-9]\""");

            FixupRegEx("startTime=\"0000-00-00T00:00:00.0000000-00:00\"", ref expectedFileData, ref fileData,
                @"startTime=\""[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9].[0-9][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]:[0-9][0-9]\""");

            FixupRegEx("endTime=\"0000-00-00T00:00:00.0000000-00:00\"", ref expectedFileData, ref fileData,
                @"endTime=\""[0-9][0-9][0-9][0-9]-[0-9][0-9]-[0-9][0-9]T[0-9][0-9]:[0-9][0-9]:[0-9][0-9].[0-9][0-9][0-9][0-9][0-9][0-9][0-9]-[0-9][0-9]:[0-9][0-9]\""");

            //fileData.Trace();
            //expectedFileData.Trace();
            fileData.ShouldEqual(expectedFileData);
        }

        private void FixupRegEx(string replacementWith, ref string s1, ref string s2, string regexPattern)
        {
            var regex = new System.Text.RegularExpressions.Regex(regexPattern);
            s1 = regex.Replace(s1, replacementWith);
            s2 = regex.Replace(s2, replacementWith);
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
                new Guid("8c84fa94-04c1-424b-9868-57a2d4851a1d"), //testListId
                new Guid("e83f94ca-b0fe-41dd-af9e-9f7adcefe7a0"), //TestRun
                new Guid("184c7c57-d71a-480f-9de6-18ff3b15a7ff"),
                new Guid("becf20cb-74e3-3a4f-2d5f-8311683824ed"),
                new Guid("06e3924c-14f5-46e7-b640-77e77598e6c0"),
                new Guid("264ee2b2-3b8f-645c-d9ff-739a31f9c1e6"),
                new Guid("e9d88bbd-1814-445e-84fb-876d083d02d9"),
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
                new Guid(),
                new Guid(),
            });
        }

        public Guid Next()
        {
            if (instance + 1 == _guids.Count)
                throw new Exception("No Guid configured for index # {0}".FormatWith(instance));
            return _guids[instance++];
        }
    }
}