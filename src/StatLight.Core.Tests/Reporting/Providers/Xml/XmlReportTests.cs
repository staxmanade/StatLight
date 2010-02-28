using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using System.Collections.Generic;
using System.Text;
using StatLight.Core.Reporting.Providers.Xml;

namespace StatLight.Core.Tests.Reporting.Providers.Xml
{
    /*

<test-results name="C:\Code\staxmanade-private\StatLight\src\StatLight.Core.Tests\StatLight.Core.Tests.nunit" total="93" failures="0" not-run="2" date="2009-04-21" time="21:20:59"> 
    <test-suite name="when_verifying_that_the_logging_works_correctly" success="True" time="0.344" asserts="0"> 
        <results> 
            <test-case name="StatLight.Core.Tests.Common.when_verifying_that_the_logging_works_correctly.verify_Debug_only_LogChatterLevel" executed="True" success="True" time="0.313" asserts="4" /> 

     * 
     * failure case below
     * 
            <test-case name="StatLight.Core.Tests.Reporting.Providers.XmlReportTests.when_verifying_the_xml_format_of_the_core_format_of_the_xml_report.the_root_element_should_have_an_attribute_called_name" executed="True" success="False" time="0.016" asserts="1"> 
                <failure> 
                    <message><![CDATA[  Expected: not null
  But was:  null
]]></message> 
                    <stack-trace><![CDATA[   at StatLight.Core.Tests.SpecificationExtensions.ShouldNotBeNull[T](T anObject) in C:\Code\staxmanade-private\StatLight\src\StatLight.Core.Tests\SpecificationExtensions.cs:line 43
   at StatLight.Core.Tests.Reporting.Providers.XmlReportTests.when_verifying_the_xml_format_of_the_core_format_of_the_xml_report.the_root_element_should_have_an_attribute_called_name() in C:\Code\staxmanade-private\StatLight\src\StatLight.Core.Tests\Reporting\Providers\XmlReport\XmlReportTests.cs:line 74
]]></stack-trace> 
                </failure> 
            </test-case> 
	 

     * 
     * Ignore case below
     * 
            <test-case name="StatLight.Core.Tests.Console.ArgOptionsTests.when_testing_the_arg_options.should_be_able_to_set_the_startServerOnly_flag" executed="False"> 
                <reason> 
                    <message><![CDATA[took this option out until I spend time putting it back in...if ever]]></message> 
                </reason> 
            </test-case>
        </results> 
    </test-suite> 
</test-results>

     */

    //    public class with_a_test_report_containing_passing_failing_and_ignore_test_results : FixtureBase
    //    {
    //        TestReport report;
    //        XmlReport xmlReport;
    //        XElement rootReport;

    //        protected TestReport Report { get { return this.report; } }
    //        protected XmlReport XmlReport { get { return this.xmlReport; } }
    //        protected XElement RootReport { get { return this.rootReport; } }

    //        protected const string TestXapFileName = "Test.xap";

    //        protected override void Before_all_tests()
    //        {
    //            base.Before_all_tests();

    //            report = new TestReport()
    //                .AddResult(MessageFactory.CreateResult(TestOutcome.Passed))
    //                .AddResult(MessageFactory.CreateResult(TestOutcome.Failed))
    //                .AddResult(MessageFactory.CreateTestIgnoreMessage("testNameHere"))
    //                ;
    //            xmlReport = new XmlReport(this.report, "Test.xap");
    //            this.rootReport = XElement.Parse(xmlReport.GetXmlReport());
    //        }
    //    }

    //    [TestFixture]
    //    [Ignore]
    //    public class when_verifying_the_xml_format_of_the_core_format_of_the_xml_report : with_a_test_report_containing_passing_failing_and_ignore_test_results
    //    {
    //        /* An example starting test result xml header
    //            <test-results 
    //                name="C:\Code\staxmanade-private\StatLight\src\StatLight.Core.Tests\StatLight.Core.Tests.nunit" 
    //                total="93"
    //                failures="0"
    //                not-run="2"
    //                date="2009-04-21"
    //                time="21:20:59">
    //            </test-result>
    //         */

    //        [Test]
    //        public void should_be_able_to_generate_an_xml_report_from_a_TestReport()
    //        {
    //            RootReport.ShouldNotBeNull();
    //        }

    //        [Test]
    //        public void the_root_element_should_be_named_testresults()
    //        {
    //            RootReport.Name.LocalName.ShouldEqual("test-results");
    //        }

    //        [Test]
    //        public void the_root_element_should_have_an_attribute_called_name()
    //        {
    //            RootReport.AttributeEquals("name", TestXapFileName);
    //            //var attribute = RootReport.Attribute("name");

    //            //attribute.ShouldNotBeNull();
    //            //attribute.Value.ShouldEqual(TestXapFileName);
    //        }

    //        [Test]
    //        public void the_root_element_should_have_an_attribute_called_total()
    //        {
    //            RootReport.AttributeEquals("total", this.Report.TotalResults);
    //            //var attribute = RootReport.Attribute("total");

    //            //attribute.ShouldNotBeNull();
    //            //attribute.Value.ShouldEqual(this.Report.TotalResults.ToString());
    //        }

    //        [Test]
    //        public void the_root_element_should_have_an_attribute_called_failures()
    //        {
    //            RootReport.AttributeEquals("failures", this.Report.TotalFailed);
    //        }

    //        [Test]
    //        public void the_root_element_should_have_an_attribute_called_not_run()
    //        {
    //            RootReport.AttributeEquals("not-run", this.Report.TotalIgnored);
    //        }

    //        [Test]
    //        public void the_root_element_should_have_an_attribute_called_date()
    //        {
    //            RootReport.AttributeEquals("date", this.Report.DateTimeRunCompleted.ToString("yyyy-MM-dd"));
    //        }

    //        [Test]
    //        public void the_root_element_should_have_an_attribute_called_time()
    //        {
    //            RootReport.AttributeEquals("time", this.Report.DateTimeRunCompleted.ToString("HH-mm-ss"));
    //        }
    //    }

    //    [TestFixture]
    //    [Ignore]
    //    public class when_verifying_the_xml_format_of_the_test_suite_element : with_a_test_report_containing_passing_failing_and_ignore_test_results
    //    {
    //        XElement testSuiteElement;

    //        protected override void Before_all_tests()
    //        {
    //            base.Before_all_tests();

    //            testSuiteElement = base.RootReport.Elements().First();
    //        }
    //        /*
    //    <test-suite name="when_verifying_that_the_logging_works_correctly" success="True" time="0.344" asserts="0"> 
    //        <results> 
    //         */

    //        [Test]
    //        public void the_element_should_be_named_test_suite()
    //        {
    //            testSuiteElement.Name.LocalName.ShouldEqual("test-suite");
    //        }

    //        [Test]
    //        public void the_root_element_should_have_an_attribute_called_success()
    //        {
    //            testSuiteElement.AttributeEquals("success", "False");
    //        }

    //        [Test]
    //        public void the_root_element_should_have_an_attribute_called_time()
    //        {
    //            testSuiteElement.AttributeEquals("time", base.Report.TimeToComplete);
    //        }

    //        [Test]
    //        [Ignore("not sure how to get the \"asserts\" flag")]
    //        public void the_root_element_should_have_an_attribute_called_asserts()
    //        {
    //            testSuiteElement.AttributeEquals("asserts", string.Empty /* TODO: */);
    //        }

    //        [Test]
    //        public void the_root_element_should_have_a_child_results_node()
    //        {
    //            var x = testSuiteElement
    //                .Descendants("results")
    //                .SingleOrDefault()
    //                .ShouldNotBeNull();
    //        }
    //    }

    //    [TestFixture]
    //    [Ignore]
    //    public class when_verifying_the_xml_format_of_a_passing_test_case_element : with_a_test_report_containing_passing_failing_and_ignore_test_results
    //    {
    //        XElement element;
    //        MobilScenarioResult passingResult;
    //        protected override void Before_all_tests()
    //        {
    //            base.Before_all_tests();

    //            passingResult = base.Report.Results
    //                .Where(w => w.Result == TestOutcome.Passed)
    //                .Single();

    //            element = base.RootReport
    //                .Elements()
    //                .First()
    //                .Descendants()
    //                .Where(w =>
    //                    w.Attributes("success")
    //                    .Any(a => a.Value == "True"))
    //                .FirstOrDefault();
    //        }

    //        /*
    //         * <test-case 
    //         *		name="StatLight.Core.Tests.Common.when_verifying_that_the_logging_works_correctly.verify_Debug_only_LogChatterLevel" 
    //         *		executed="True" 
    //         *		success="True" 
    //         *		time="0.313" 
    //         *		asserts="4" /> 
    //         */
    //        [Test]
    //        public void should_contain_a_successful_element()
    //        {
    //            element.ShouldNotBeNull();
    //        }

    //        [Test]
    //        public void element_name_should_be_test_case()
    //        {
    //            element.Name.LocalName.ShouldEqual("test-case");
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_success()
    //        {
    //            element.AttributeEquals("success", "True");
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_name()
    //        {
    //            element.AttributeEquals("name", passingResult.TestClassName + "." + passingResult.TestName);
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_executed()
    //        {
    //            element.AttributeEquals("executed", "True");
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_time()
    //        {
    //            element.AttributeEquals("time", passingResult.TimeToComplete);
    //        }
    //    }

    //    [TestFixture]
    //    [Ignore]
    //    public class when_verifying_the_xml_format_of_a_failing_test_case_element : with_a_test_report_containing_passing_failing_and_ignore_test_results
    //    {
    //        XElement element;
    //        MobilScenarioResult failingResult;
    //        protected override void Before_all_tests()
    //        {
    //            base.Before_all_tests();

    //            failingResult = base.Report.Results
    //                .Where(w => w.Result == TestOutcome.Failed)
    //                .Single();

    //            element = base.RootReport
    //                .Elements()
    //                .First()
    //                .Descendants()
    //                .Where(w =>
    //                    w.Attributes("success")
    //                    .Any(a => a.Value == "False"))
    //                .FirstOrDefault();
    //        }

    //        /*
    //            <test-case name="StatLight.Core.Tests.Reporting.Providers.XmlReportTests.when_verifying_the_xml_format_of_the_core_format_of_the_xml_report.the_root_element_should_have_an_attribute_called_name" executed="True" 
    //         * success="False" 
    //         * time="0.016" 
    //         * asserts="1"> 
    //                <failure> 
    //                    <message><![CDATA[  Expected: not null
    //  But was:  null
    //]]></message> 
    //                    <stack-trace><![CDATA[   at StatLight.Core.Tests.SpecificationExtensions.ShouldNotBeNull[T](T anObject) in C:\Code\staxmanade-private\StatLight\src\StatLight.Core.Tests\SpecificationExtensions.cs:line 43
    //   at StatLight.Core.Tests.Reporting.Providers.XmlReportTests.when_verifying_the_xml_format_of_the_core_format_of_the_xml_report.the_root_element_should_have_an_attribute_called_name() in C:\Code\staxmanade-private\StatLight\src\StatLight.Core.Tests\Reporting\Providers\XmlReport\XmlReportTests.cs:line 74
    //]]></stack-trace> 
    //                </failure> 
    //            </test-case> 
    //         */
    //        [Test]
    //        public void should_contain_a_successful_element()
    //        {
    //            element.ShouldNotBeNull();
    //        }

    //        [Test]
    //        public void element_name_should_be_test_case()
    //        {
    //            element.Name.LocalName.ShouldEqual("test-case");
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_success()
    //        {
    //            element.AttributeEquals("success", "False");
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_name()
    //        {
    //            element.AttributeEquals("name", failingResult.TestClassName + "." + failingResult.TestName);
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_executed()
    //        {
    //            element.AttributeEquals("executed", "True");
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_time()
    //        {
    //            element.AttributeEquals("time", failingResult.TimeToComplete);
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_child_element_called_reason()
    //        {
    //            element.Descendants("failure").SingleOrDefault()
    //                .ShouldNotBeNull();
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_child_element_called_reason_that_has_an_element_called_message()
    //        {
    //            element.Descendants("failure").Single()
    //                .Descendants("message").SingleOrDefault()
    //                .ShouldNotBeNull();
    //        }

    //        [Test]
    //        [Ignore("Figure out how to get the stack-trace out of a failure message.")]
    //        public void the_test_result_element_should_have_child_element_called_reason_that_has_an_element_called_stack_trace()
    //        {
    //            element.Descendants("failure").Single()
    //                .Descendants("stack-trace").SingleOrDefault()
    //                .ShouldNotBeNull();
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_child_element_called_reason_that_has_an_element_called_message_thats_message_CData_is_the_OtherMessages_message_value()
    //        {
    //            element.Descendants("failure").Single()
    //                .Descendants("message").SingleOrDefault()
    //                .ShouldNotBeNull()
    //                .Value.ShouldEqual(failingResult.ExceptionMessage);
    //        }
    //    }

    //    [TestFixture]
    //    [Ignore]
    //    public class when_verifying_the_xml_format_of_an_ignore_test_case_element : with_a_test_report_containing_passing_failing_and_ignore_test_results
    //    {
    //        XElement element;
    //        MobilOtherMessageType ignoreResult;

    //        protected override void Before_all_tests()
    //        {
    //            base.Before_all_tests();

    //            ignoreResult = base.Report.OtherMessages
    //                .Where(w => w.IsIgnoreMessage())
    //                .Single();

    //            element = base.RootReport
    //                .Elements()
    //                .First()
    //                .Descendants()
    //                .Where(w =>
    //                    w.Attributes("executed")
    //                    .Any(a => a.Value == "False"))
    //                .FirstOrDefault();
    //        }

    //        /*
    //            <test-case name="StatLight.Core.Tests.Console.ArgOptionsTests.when_testing_the_arg_options.should_be_able_to_set_the_startServerOnly_flag" 
    //         * executed="False"> 
    //                <reason> 
    //                    <message><![CDATA[took this option out until I spend time putting it back in...if ever]]></message> 
    //                </reason> 
    //            </test-case>
    //         */

    //        [Test]
    //        public void should_contain_an_ignore_element()
    //        {
    //            element.ShouldNotBeNull();
    //        }

    //        [Test]
    //        public void element_name_should_be_test_case()
    //        {
    //            element.Name.LocalName.ShouldEqual("test-case");
    //        }

    //        [Test]
    //        [Ignore("Figure out how to extract the namespace.class.method out of an ignored test")]
    //        public void the_test_result_element_should_have_an_attribute_called_name()
    //        {
    //            element.AttributeEquals("name", "TODO");
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_an_attribute_called_executed()
    //        {
    //            element.AttributeEquals("executed", "False");
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_child_element_called_reason()
    //        {
    //            element.Descendants("reason").SingleOrDefault()
    //                .ShouldNotBeNull();
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_child_element_called_reason_that_has_an_element_called_message()
    //        {
    //            element.Descendants("reason").Single()
    //                .Descendants("message").SingleOrDefault()
    //                .ShouldNotBeNull();
    //        }

    //        [Test]
    //        public void the_test_result_element_should_have_child_element_called_reason_that_has_an_element_called_message_thats_message_CData_is_the_OtherMessages_message_value()
    //        {
    //            element.Descendants("reason").Single()
    //                .Descendants("message").SingleOrDefault()
    //                .ShouldNotBeNull()
    //                .Value.ShouldEqual(ignoreResult.Message);
    //        }

    //    }

    //    static class testExtensions
    //    {
    //        public static XElement AttributeEquals(this XElement element, string elementName, object value)
    //        {
    //            var attribute = element.Attribute(elementName);

    //            attribute.ShouldNotBeNull();
    //            attribute.Value.ShouldEqual(value.ToString());
    //            return element;
    //        }
    //    }
    //[TestFixture]
    //public class go
    //{
    //    [Test]
    //    public void Should_TestName()
    //    {
    //        const string xmlPath = @"C:\Code\StatLight.Git\hi.xml";

    //        IList<string> validationErrors;
    //        if (!XmlReport.ValidateSchema(xmlPath, out validationErrors))
    //            Assert.Fail(string.Join(Environment.NewLine, validationErrors.ToArray()));
    //    }
    //}
}


