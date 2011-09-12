using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using StatLight.Core.Events;

namespace StatLight.Core.Reporting.Providers.MSTestTRX
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TRX")]
    public class TRXReport : IXmlReport
    {
        private static readonly Dictionary<TestCaseResult, Guid> _executionIdHash = new Dictionary<TestCaseResult, Guid>();
        private static readonly Dictionary<TestCaseResult, Guid> _testIdHash = new Dictionary<TestCaseResult, Guid>();
        private readonly TestReportCollection _report;
        private readonly IGuidSequenceGenerator _guidSequenceGenerator;
        private readonly TestSettings _testSettings;
        private Guid _testListId;

        public TRXReport(TestReportCollection report)
            : this(report, new GuidSequenceGenerator(), new TestSettings())
        {
        }

        public TRXReport(TestReportCollection report, IGuidSequenceGenerator guidSequenceGenerator, TestSettings testSettings)
        {
            if (report == null)
                throw new ArgumentNullException("report");

            _report = report;
            _guidSequenceGenerator = guidSequenceGenerator;
            _testSettings = testSettings;
        }


        public void WriteXmlReport(string outputFilePath)
        {
            using (var writer = new StreamWriter(outputFilePath))
            {
                var xml = GetXmlReport();
                xml.Save(writer);
                writer.Close();
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public XDocument GetXmlReport()
        {
            var results = _report;

            var testListId = _guidSequenceGenerator.Next();

            var ns = XNamespace.Get(@"http://microsoft.com/schemas/VisualStudio/TeamTest/2010");

            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "")
                , new XElement(ns + "TestRun"
                    , new XAttribute("id", _guidSequenceGenerator.Next().ToString())
                    , new XAttribute("name", "StatLight TestRun")
                    , new XAttribute("runUser", @"DOMAIN\UserName")
                    , new XElement(ns + "TestSettings",
                        new XAttribute("name", _testSettings.Name)
                        , new XAttribute("id", _guidSequenceGenerator.Next())
                        , new XElement(ns + "Description", _testSettings.Description)
                        , new XElement(ns + "Deployment"
                            , new XAttribute("enabled", _testSettings.DeploymentEnabled)
                            , new XAttribute("runDeploymentRoot", _testSettings.DeploymentRunDeploymentRoot))
                //,new XElement("Deployment", new XAttribute("enabled", false))
                        , new XElement(ns + "Execution"
                            , new XElement(ns + "TestTypeSpecific")
                            , new XElement(ns + "AgentRule", new XAttribute("name", "Execution Agents"))
                            )
                    )
                    , new XElement(ns + "Times"
                        , new XAttribute("creation", _testSettings.TimesCreation)
                        , new XAttribute("queuing", _testSettings.TimesQueuing)
                        , new XAttribute("start", _testSettings.TimesStart)
                        , new XAttribute("finish", _testSettings.TimesFinish)
                    )
                    , new XElement(ns + "ResultSummary",
                        new XAttribute("outcome", (results.FinalResult == RunCompletedState.Failure ? "Failed" : "Completed")),
                        new XElement(ns + "Counters",
                            new XAttribute("total", results.TotalResults),
                            new XAttribute("executed", results.TotalResults - results.TotalIgnored),
                            new XAttribute("passed", results.TotalPassed),
                            new XAttribute("error", 0),
                            new XAttribute("failed", results.TotalFailed),
                            new XAttribute("timeout", 0),
                            new XAttribute("aborted", 0),
                            new XAttribute("inconclusive", 0),
                            new XAttribute("passedButRunAborted", 0),
                            new XAttribute("notRunnable", results.TotalIgnored),
                            new XAttribute("notExecuted", 0),
                            new XAttribute("disconnected", 0),
                            new XAttribute("warning", 0),
                            new XAttribute("completed", 0),
                            new XAttribute("inProgress", 0),
                            new XAttribute("pending", 0)
                        )
                    )
                    , new XElement(ns + "TestDefinitions"
                        , from test in results.AllTests()
                          select new XElement(ns + "UnitTest"
                              , new XAttribute("name", test.MethodName)
                              , new XAttribute("id", GetTestId(test))
                              , new XElement(ns + "Execution"
                                  , new XAttribute("id", GetExecutionId(test))
                                )
                              , new XElement(ns + "TestMethod"
                                  , new XAttribute("codeBase", string.Empty)
                                  , new XAttribute("adapterTypeName", string.Empty)
                                  , new XAttribute("className", test.NamespaceName + "." + test.ClassName)
                                  , new XAttribute("name", test.MethodName)
                                )
                            )
                        )
                    , new XElement(ns + "TestLists"
                        , new XElement(ns + "TestList"
                            , new XAttribute("name", "Results Not in a List")
                            , new XAttribute("id", (_testListId = _guidSequenceGenerator.Next()))
                            )
                        )
                //,GetTestDefinitions(results)
                    , new XElement(ns + "TestEntries"
                        , from test in results.AllTests()
                          select new XElement(ns + "TestEntry"
                                , new XAttribute("testId", GetTestId(test))
                                , new XAttribute("executionId", GetExecutionId(test))
                                , new XAttribute("testListId", testListId)
                            )
                        )
                    , GetResults(results, ns)
                    )
                );

            //doc.Root.SetAttributeValue("xmlns", @"http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            return doc;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.TimeSpan.ToString(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private XElement GetResults(TestReportCollection results, XNamespace ns)
        {
            Func<ResultType, string> getResult = r =>
            {
                if (r == ResultType.Failed)
                    return "Failed";
                if (r == ResultType.SystemGeneratedFailure)
                    return "Failed";
                if (r == ResultType.Passed)
                    return "Passed";

                if (r == ResultType.Ignored)
                    return "Ignored";

                throw new NotImplementedException("Unknown result type [{0}]".FormatWith(r.ToString()));
            };

            return new XElement(ns + "Results",
                from test in results.AllTests()
                let output = (test.ResultType == ResultType.Failed ? new XElement(ns + "Output"
                    , new XElement(ns + "ErrorInfo"
                        , new XElement(ns + "Message"
                            , test.ExceptionInfo.Message)
                        , new XElement(ns + "StackTrace"
                            , test.ExceptionInfo.StackTrace)
                        )) : null)
                select new XElement(ns + "UnitTestResult"
                    , new XAttribute("executionId", GetExecutionId(test))
                    , new XAttribute("testId", GetTestId(test))
                    , new XAttribute("testName", test.MethodName)
                    , new XAttribute("computerName", _testSettings.ComputerName)
                    , new XAttribute("duration", test.TimeToComplete.ToString("hh\\:mm\\:ss\\.fffffff"))
                    , new XAttribute("startTime", test.Started.ToString("yyyy-MM-ddThh:mm:ss.fffffffK"))
                    , new XAttribute("endTime", (test.Finished ?? new DateTime()).ToString("yyyy-MM-ddThh:mm:ss.fffffffK"))
                    , new XAttribute("testType", _testSettings.TestType)
                    , new XAttribute("outcome", getResult(test.ResultType))
                    , new XAttribute("testListId", _testListId)
                    , new XAttribute("relativeResultsDirectory", GetExecutionId(test))
                    ,output
                )
            );
        }


        private Guid GetExecutionId(TestCaseResult testCaseResult)
        {
            return GetGuidForItem(testCaseResult, HashType.ExecutionId);
        }

        private Guid GetTestId(TestCaseResult testCaseResult)
        {
            return GetGuidForItem(testCaseResult, HashType.TestId);
        }

        private enum HashType
        {
            ExecutionId,
            TestId,
        }

        private Guid GetGuidForItem(TestCaseResult testCaseResult, HashType hashType)
        {
            IDictionary<TestCaseResult, Guid> hash;
            switch (hashType)
            {
                case HashType.ExecutionId:
                    hash = _executionIdHash;
                    break;
                case HashType.TestId:
                    hash = _testIdHash;
                    break;
                default:
                    throw new NotImplementedException();
            }

            Guid newGuid;
            if (hash.ContainsKey(testCaseResult))
                newGuid = hash[testCaseResult];
            else
            {
                newGuid = _guidSequenceGenerator.Next();
                hash.Add(testCaseResult, newGuid);
            }
            return newGuid;
        }


        //private static string GetErrorStackTrace(TestCaseResult testCaseResult)
        //{
        //    if (testCaseResult.ExceptionInfo != null)
        //        return testCaseResult.ExceptionInfo.StackTrace ?? "";
        //    return testCaseResult.OtherInfo ?? "";
        //}

        //private static string GetErrorMessage(TestCaseResult r)
        //{
        //    if (r.ExceptionInfo != null)
        //        return r.ExceptionInfo.FullMessage ?? "";
        //    return r.OtherInfo ?? "";
        //}
    }

    public class TestSettings
    {
        public string Name = "Local";
        public string Description = "These are default test settings for a local test run.";
        public bool DeploymentEnabled = false;
        public string DeploymentRunDeploymentRoot = "UserName_UserName-LT3 2011-08-23 11_36_44";
        public string TimesCreation = "2011-08-23T11:36:44.4831051-07:00";
        public string TimesQueuing = "2011-08-23T11:36:45.2943065-07:00";
        public string TimesStart = "2011-08-23T11:36:45.3567066-07:00";
        public string TimesFinish = "2011-08-23T11:36:45.8715075-07:00";
        public string ComputerName = Environment.MachineName;
        public string TestType = "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b";
    }

    public class GuidSequenceGenerator : IGuidSequenceGenerator
    {
        public Guid Next()
        {
            return Guid.NewGuid();
        }
    }

    public interface IGuidSequenceGenerator
    {
        Guid Next();
    }

}