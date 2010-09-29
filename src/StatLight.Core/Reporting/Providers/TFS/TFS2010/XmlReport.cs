

namespace StatLight.Core.Reporting.Providers.TFS.TFS2010
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public class XmlReport
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
            Func<IEnumerable<TestCaseResult>, IEnumerable<XElement>> getInnerTests = result =>
                result.Select(s =>
                    new XElement("InnerTest",
                        new XElement("TestName", s.FullMethodName()),
                        new XElement("TestResult", GetTestResult(s.ResultType)),
                        new XElement("ErrorMessage", s.ExceptionInfo == null ? "" : s.ExceptionInfo.ToString())
                        ));

            var firstReport = _report.First();
            var report = new XElement("SummaryResult",
                                      new XElement("TestName", "StatLight Tests"),
                                      new XElement("TestResult", GetFinalResult()),
                                      new XElement("InnerTests", getInnerTests(firstReport.TestResults)));
            return report.ToString();
        }

        private static TestResultType GetTestResult(ResultType resultType)
        {
            switch (resultType)
            {
                case ResultType.Passed:
                    return TestResultType.Passed;
                default:
                    return TestResultType.Failed;
            }
        }

        private TestResultType GetFinalResult()
        {
            if (_report.FinalResult == RunCompletedState.Failure)
            {
                return TestResultType.Failed;
            }
            return TestResultType.Passed;
        }
    }
}
