using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using StatLight.Core.Events;
using StatLight.Core.Common;

namespace StatLight.Core.Reporting.Providers.MSTestTRX
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "TRX")]
    public class TRXReport : IXmlReport
    {
        private static readonly Dictionary<TestCaseResultServerEvent, Guid> _executionIdHash = new Dictionary<TestCaseResultServerEvent, Guid>();
        private static readonly Dictionary<TestCaseResultServerEvent, Guid> _testIdHash = new Dictionary<TestCaseResultServerEvent, Guid>();
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
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public XDocument GetXmlReport()
        {
            var results = _report;

            _testListId = _guidSequenceGenerator.GetNext();

            var ns = XNamespace.Get(@"http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            
            var doc = new XDocument(
                new XDeclaration("1.0", "UTF-8", "")
                , new XElement(ns + "TestRun"
                    , new XAttribute("id", _guidSequenceGenerator.GetNext().ToString())
                    , new XAttribute("name", "StatLight TestRun")
                    , new XAttribute("runUser", string.Format(CultureInfo.InvariantCulture, @"{0}\{1}", Environment.UserDomainName, Environment.UserName))
                    , new XElement(ns + "TestSettings",
                        new XAttribute("name", _testSettings.Name)
                        , new XAttribute("id", _guidSequenceGenerator.GetNext())
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
                        , from test in GetTRXTests(results)
                          select new XElement(ns + "UnitTest"
                              , new XAttribute("name", test.MethodName)
                              , new XAttribute("id", GetTestId(test))
                              , new XElement(ns + "Execution"
                                  , new XAttribute("id", GetExecutionId(test))
                                )
                              , new XElement(ns + "TestMethod"
                                  , new XAttribute("codeBase", test.XapFilePath)
                                  , new XAttribute("adapterTypeName", string.Empty)
                                  , new XAttribute("className", test.NamespaceName + "." + test.ClassName)
                                  , new XAttribute("name", test.MethodName)
                                )
                              , GetTestOwner(test, ns)
                            )
                        )
                    , new XElement(ns + "TestLists"
                        , new XElement(ns + "TestList"
                            , new XAttribute("name", "StatLight Test List")
                            , new XAttribute("id", _testListId)
                            )
                        )
                //,GetTestDefinitions(results)
                    , new XElement(ns + "TestEntries"
                        , from test in GetTRXTests(results)
                          select new XElement(ns + "TestEntry"
                                , new XAttribute("testId", GetTestId(test))
                                , new XAttribute("executionId", GetExecutionId(test))
                                , new XAttribute("testListId", _testListId)
                            )
                        )
                    , GetResults(results, ns)
                    )
                );
            
            //doc.Root.SetAttributeValue("xmlns", @"http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            return doc;
        }

        private static IEnumerable<TestCaseResultServerEvent> GetTRXTests(TestReportCollection testReportCollection)
        {
            return testReportCollection
                .AllTests()
                .Where(w => w.ResultType != ResultType.Ignored);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "haha"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.DateTime.ToString(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1305:SpecifyIFormatProvider", MessageId = "System.TimeSpan.ToString(System.String)"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
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
                    throw new StatLightException("TRX doesn't have an Ignored type - this test should have not come up and should be - haha 'Ignored'.");

                throw new NotImplementedException("Unknown result type [{0}]".FormatWith(r.ToString()));
            };


            Func<TestCaseResultServerEvent, XElement> getOutput = test =>
            {
                Func<Func<ExceptionInfo, string>, string> getExceptionInfo = (getter) =>
                {
                    if (test.ExceptionInfo == null)
                        return "";

                    return getter(test.ExceptionInfo) ?? "";
                };

                Func<string> getOtherInfo = () =>
                {
                    if (string.IsNullOrEmpty(test.OtherInfo))
                        return "";

                    return Environment.NewLine + "Other Info: " + test.OtherInfo;
                };

                if (test.ResultType == ResultType.Failed || test.ResultType == ResultType.SystemGeneratedFailure)
                {
                    return new XElement(ns + "Output"
                                        , new XElement(ns + "ErrorInfo"
                                                       , new XElement(ns + "Message"
                                                                      ,
                                                                      getExceptionInfo(x => x.Message) + getOtherInfo())
                                                       , new XElement(ns + "StackTrace"
                                                                      , getExceptionInfo(x => x.StackTrace))
                                              ));
                }
                return null;
            };

            return new XElement(ns + "Results",
                from test in GetTRXTests(results)
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
                    , getOutput(test)
                )
            );
        }

        private static XElement GetTestOwner(TestCaseResultServerEvent test, XNamespace ns)
        {
            var owners = test.Metadata.Where(i => i.Value == "Owner");
            var items = new List<XElement>();
            foreach (var owner in owners)
            {
                items.Add(new XElement(ns + "Owner",
                    new XAttribute("name", owner.Name)));
            }
            if (items.Count > 0)
                return new XElement(ns + "Owners", items);

            return null;
        }

        private Guid GetExecutionId(TestCaseResultServerEvent testCaseResultServerEvent)
        {
            return GetGuidForItem(testCaseResultServerEvent, HashType.ExecutionId);
        }

        private Guid GetTestId(TestCaseResultServerEvent testCaseResultServerEvent)
        {
            return GetGuidForItem(testCaseResultServerEvent, HashType.TestId);
        }

        private enum HashType
        {
            ExecutionId,
            TestId,
        }

        private Guid GetGuidForItem(TestCaseResultServerEvent testCaseResultServerEvent, HashType hashType)
        {
            IDictionary<TestCaseResultServerEvent, Guid> hash;
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
            if (hash.ContainsKey(testCaseResultServerEvent))
                newGuid = hash[testCaseResultServerEvent];
            else
            {
                newGuid = _guidSequenceGenerator.GetNext();
                hash.Add(testCaseResultServerEvent, newGuid);
            }
            return newGuid;
        }


        //private static string GetErrorStackTrace(TestCaseResultServerEvent testCaseResult)
        //{
        //    if (testCaseResult.ExceptionInfo != null)
        //        return testCaseResult.ExceptionInfo.StackTrace ?? "";
        //    return testCaseResult.OtherInfo ?? "";
        //}

        //private static string GetErrorMessage(TestCaseResultServerEvent r)
        //{
        //    if (r.ExceptionInfo != null)
        //        return r.ExceptionInfo.FullMessage ?? "";
        //    return r.OtherInfo ?? "";
        //}
    }

    public class TestSettings
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string Name = "Local";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string Description = "These are default test settings for a local test run.";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public bool DeploymentEnabled = false;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string DeploymentRunDeploymentRoot = "UserName_UserName-LT3 2011-08-23 11_36_44";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string TimesCreation = "2011-08-23T11:36:44.4831051-07:00";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string TimesQueuing = "2011-08-23T11:36:45.2943065-07:00";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string TimesStart = "2011-08-23T11:36:45.3567066-07:00";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string TimesFinish = "2011-08-23T11:36:45.8715075-07:00";
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string ComputerName = Environment.MachineName;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
        public string TestType = "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b";
    }

    public class GuidSequenceGenerator : IGuidSequenceGenerator
    {
        public Guid GetNext()
        {
            return Guid.NewGuid();
        }
    }

    public interface IGuidSequenceGenerator
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        Guid GetNext();
    }

}
