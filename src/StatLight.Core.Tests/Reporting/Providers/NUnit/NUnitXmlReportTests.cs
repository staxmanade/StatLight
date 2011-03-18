using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;
using StatLight.Core.Reporting;
using StatLight.Core.Reporting.Providers.NUnit;
using StatLight.Core.Tests.Reporting.Providers.Xml;

namespace StatLight.Core.Tests.Reporting.Providers
{

    public class when_validating_the_NUnitXmlReport_is_correct : FixtureBase
    {
        private NUnitXmlReport _xmlReport;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            Func<ResultType, ExceptionInfo, TestCaseResult> getResult = (resultType, exceptionInfo) =>
            {
                return new TestCaseResult(resultType)
                {
                    ClassName = "class_name",
                    MethodName = "method_name",
                    NamespaceName = "namespace.here",
                    Finished = new DateTime(2009, 2, 2, 2, 2, 2),
                    Started = new DateTime(2009, 2, 2, 2, 2, 1),
                    ExceptionInfo = exceptionInfo,
                };
            };

            var testReport = new TestReport("Test.xap")
                .AddResult(getResult(ResultType.Passed, null))
                .AddResult(getResult(ResultType.Failed, new ExceptionInfo(GetException())))
                .AddResult(getResult(ResultType.Ignored, null))
                .AddResult(getResult(ResultType.SystemGeneratedFailure, new ExceptionInfo(new Exception("fail"))))
                ;

            _xmlReport = new NUnitXmlReport(testReport.ToTestReportCollection());
        }

        private static Exception GetException()
        {
            Exception ex1;
            try
            {
                throw new Exception("exception1");
            }
            catch (Exception ex)
            {
                ex1 = ex;
            }
            Exception ex2;
            try
            {
                throw new Exception("exception2", ex1);
            }
            catch (Exception ex)
            {
                ex2 = ex;
            }
            return ex2;
        }

        [Test]
        [Ignore]
        public void Should_pass_the_schema_validation()
        {
            var file = Path.GetTempFileName();

            using (TextWriter tw = new StreamWriter(file))
            {
                tw.Write(_xmlReport.GetXmlReport());
            }

            IList<string> errors;
            if (!NUnitXmlReport.ValidateSchema(file, out errors))
            {
                var msg = string.Join(Environment.NewLine, errors.ToArray());
                Assert.Fail(msg);
            }
        }

        [Test]
        [Ignore]
        public void Should_not_throw_any_exceptions_if_there_are_no_tests()
        {
            NUnitXmlReport xmlReport = new NUnitXmlReport(new TestReport("Test.xap").ToTestReportCollection());
            xmlReport.GetXmlReport().ShouldNotBeEmpty();
        }
    }
}