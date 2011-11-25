using System;
using System.Linq;
using System.Collections.Generic;
using NUnit.Framework;
using StatLight.Client.Harness.Events;
using StatLight.Core.Configuration;
using StatLight.Core.Events;
using StatLight.Core.Tests;
using StatLight.Core.Reporting;
using StatLight.Core.Events;

namespace StatLight.IntegrationTests.ProviderTests.MSTest
{
    [TestFixture]
    public class when_testing_the_runner_with_MSTest_tests_sl3
        : IntegrationFixtureBase
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;
        private InitializationOfUnitTestHarnessClientEvent _initializationOfUnitTestHarnessClientEvent;

        private readonly IList<TestExecutionClassCompletedClientEvent> _testExecutionClassCompletedClientEvent = new List<TestExecutionClassCompletedClientEvent>();
        private readonly IList<TestExecutionMethodIgnoredClientEvent> _testExecutionMethodIgnoredClientEvent = new List<TestExecutionMethodIgnoredClientEvent>();
        private readonly IList<TestExecutionMethodFailedClientEvent> _testExecutionMethodFailedClientEvent = new List<TestExecutionMethodFailedClientEvent>();
        private readonly IList<TestExecutionMethodPassedClientEvent> _testExecutionMethodPassedClientEvent = new List<TestExecutionMethodPassedClientEvent>();

        protected override ClientTestRunConfiguration ClientTestRunConfiguration
        {
            get { return _clientTestRunConfiguration; }
        }

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            PathToIntegrationTestXap = TestXapFileLocations.MSTestSL3;
            _clientTestRunConfiguration = new IntegrationTestClientTestRunConfiguration();
            EventSubscriptionManager.AddListenerAction<InitializationOfUnitTestHarnessClientEvent>(e => _initializationOfUnitTestHarnessClientEvent = e);
            EventSubscriptionManager.AddListenerAction<TestExecutionClassCompletedClientEvent>(e => _testExecutionClassCompletedClientEvent.Add(e));
            EventSubscriptionManager.AddListenerAction<TestExecutionMethodIgnoredClientEvent>(e => _testExecutionMethodIgnoredClientEvent.Add(e));
            EventSubscriptionManager.AddListenerAction<TestExecutionMethodFailedClientEvent>(e => _testExecutionMethodFailedClientEvent.Add(e));
            EventSubscriptionManager.AddListenerAction<TestExecutionMethodPassedClientEvent>(e => _testExecutionMethodPassedClientEvent.Add(e));

        }


        [Test]
        public void Should_have_correct_TotalFailed_count()
        {
#if DEBUG
            TestReport.TotalFailed.ShouldEqual(5);
#else
            TestReport.TotalFailed.ShouldEqual(4);
#endif
        }

        [Test]
        public void Should_have_correct_TotalPassed_count_except_theres_one_extra_passed_test_here_because_of_the_MessageBox_test()
        {
            TestReport.TotalPassed.ShouldEqual(9);
        }

        [Test]
        public void Should_have_correct_TotalIgnored_count()
        {
            TestReport.TotalIgnored.ShouldEqual(1);
        }

        #region Events

        [Test]
        public void Should_receive_one_InitializationOfUnitTestHarness()
        {
            _initializationOfUnitTestHarnessClientEvent.ShouldNotBeNull();
        }

        [Test]
        public void Should_receive_the_TestExecutionClassCompletedClientEvent()
        {
            _testExecutionClassCompletedClientEvent.Count().ShouldEqual(2);
            _testExecutionClassCompletedClientEvent.Each(AssertTestExecutionClassData);
        }

        [Test]
        public void Should_receive_the_TestExecutionMethodIgnoredClientEvent()
        {
            _testExecutionMethodIgnoredClientEvent.Count().ShouldEqual(1);
            _testExecutionMethodIgnoredClientEvent.First().MethodName.ShouldEqual("this_should_be_an_Ignored_test");
            _testExecutionMethodIgnoredClientEvent.First().Message.ShouldEqual("this_should_be_an_Ignored_test");
            //AssertTestExecutionClassData(_testExecutionMethodIgnoredClientEvent.First());
            //TODO: figure out how to get the class/namespace for the ignored test.
        }

        [Test]
        public void Should_receive_the_TestExecutionMethodFailedClientEvent()
        {
            _testExecutionMethodFailedClientEvent.Count().ShouldEqual(3);

            var e = _testExecutionMethodFailedClientEvent.First();

            AssertTestExecutionClassData(e);
            //TODO: assert other properties of the failed exception?

            e.Finished.ShouldNotEqual(new DateTime());
            e.Started.ShouldNotEqual(new DateTime());
        }

        [Test]
        public void Should_receive_the_TestExecutionMethodPassedClientEvent()
        {
#if DEBUG
            _testExecutionMethodPassedClientEvent.Count.ShouldEqual(10);
#else
            _testExecutionMethodPassedClientEvent.Count.ShouldEqual(9);
#endif
        }

        private static void AssertTestExecutionClassData(TestExecutionClass e)
        {
            e.NamespaceName.ShouldEqual("StatLight.IntegrationTests.Silverlight", "{0} - NamespaceName property should be correct.".FormatWith(e.GetType().FullName));

            var validClassNames = new List<string> { "MSTestTests+MSTestNestedClassTests", "MSTestTests" };
            if (!validClassNames.Contains(e.ClassName))
                Assert.Fail("e.ClassName is not equal to MSTestNestedClassTests or MSTestTest - actual=" + e.ClassName);
        }
        #endregion

        [Test]
        public void Should_have_reported_a_timeout_failure_correctly()
        {
            var failedTimeoutResult = TestReport.TestResults.SingleOrDefault(w => w.HasExceptionInfoWithCriteria(ex => ex.FullMessage.Contains("Timeout")));
            failedTimeoutResult.ShouldNotBeNull();
        }
#if DEBUG
        [Test]
        public void Should_have_reported_a_debug_assertion_error()
        {
            var assertionResult = TestReport
                .TestResults
                .Single(w => (w.MethodName != null ? w.MethodName.Equals("Should_fail_due_to_a_dialog_assertion") : false));

            assertionResult
                .OtherInfo
                .ShouldContain("Debug Assertion")
                .ShouldContain("Should_fail_due_to_a_dialog_assertion - message");

            assertionResult.ResultType.ShouldEqual(ResultType.Failed);
        }
#endif
        [Test]
        public void Should_have_scraped_the__messageBox_overload_1__test_message_box_info()
        {
            var nonEmptyOtherInfoResults = TestReport.TestResults.Where(w => !string.IsNullOrEmpty(w.OtherInfo));
            var theOneWeWant = nonEmptyOtherInfoResults.Single(w => w.OtherInfo.Contains("Should_fail_due_to_a_message_box_modal_dialog"));
            theOneWeWant.OtherInfo.ShouldContain("Should_fail_due_to_a_message_box_modal_dialog - message");

            theOneWeWant.ResultType.ShouldEqual(ResultType.SystemGeneratedFailure);
        }


        [Test]
        public void Should_have_pulled_the_DescriptionAttribute_information_out_of_a_failing_test()
        {
            TestReport
                .TestResults
                .Where(w => w.MethodName.Equals("this_should_be_a_Failing_test"))
                .Each(theOneWeWant => theOneWeWant.ShouldNotBeNull().ReadMetadata("Description").Each(x => x.ShouldEqual("Test description on failing test.")));
        }


        [Test]
        public void Should_have_pulled_the_OwnerAttribute_information_out_of_a_failing_test()
        {
            TestReport
                .TestResults
                .Where(w => w.MethodName.Equals("this_should_be_a_Failing_test"))
                .Each(theOneWeWant => theOneWeWant.ShouldNotBeNull().ReadMetadata("Owner").Each(x => x.ShouldEqual("SomeOwnerString")));

        }


        [Test]
        public void Should_have_pulled_the_DescriptionAttribute_information_out_of_a_passing_test()
        {
            TestReport
                .TestResults
                .Where(w => w.MethodName.Equals("this_should_be_a_passing_test") && w.ClassName.Equals("MSTestTests"))
                .Each(theOneWeWant => theOneWeWant.ShouldNotBeNull().ReadMetadata("Description").Each(x => x.ShouldEqual("Test description on failing test.")));
        }


        [Test]
        public void Should_have_pulled_the_OwnerAttribute_information_out_of_a_passing_test()
        {
            TestReport
                .TestResults
                .Where(w => w.MethodName.Equals("this_should_be_a_passing_test") && w.ClassName.Equals("MSTestTests"))
                .Each(theOneWeWant => theOneWeWant.ShouldNotBeNull().ReadMetadata("Owner").Each(x => x.ShouldEqual("SomeOwnerString")));
        }

        [Test]
        public void Should_have_pulled_the_PropertyAttribute_information_out_of_a_passing_test()
        {
            TestReport
                .TestResults
                .Where(w => w.MethodName.Equals("this_should_be_a_passing_test") && w.ClassName.Equals("MSTestTests"))
                .Each(theOneWeWant => theOneWeWant.ShouldNotBeNull().ReadMetadata("tpName").Each(x => x.ShouldEqual("tpValue")));
        }
    }
}
