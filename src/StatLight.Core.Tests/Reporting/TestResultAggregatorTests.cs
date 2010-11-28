using System;
using System.Linq;
using NUnit.Framework;
using StatLight.Client.Harness.Events;
using StatLight.Core.Events;
using StatLight.Core.Reporting;
using System.Collections.Generic;
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Tests.Reporting
{
    namespace TestResultAggregatorTests
    {
        public abstract class for_a_TestResultAggregator_that_should_handle_a_ClientEvent : FixtureBase
        {
            public TestResultAggregator TestResultAggregator { get; private set; }

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                TestResultAggregator = new TestResultAggregator(TestLogger, base.TestEventPublisher, "test");
            }
        }

        [TestFixture]
        public class when_a_TestExecutionMethodPassedClientEvent_was_published : for_a_TestResultAggregator_that_should_handle_a_ClientEvent
        {
            private TestCaseResult _passedResult;
            private TestExecutionMethodPassedClientEvent _testExecutionMethodPassedClientEvent;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                _testExecutionMethodPassedClientEvent = new TestExecutionMethodPassedClientEvent()
                {
                    ClassName = "Class name test",
                    MethodName = "method name test",
                    NamespaceName = "namespace test",
                    Finished = new DateTime(2009, 1, 1, 1, 1, 1),
                    Started = new DateTime(2009, 1, 1, 1, 1, 2),
                };
            }

            protected override void Because()
            {
                base.Because();
                TestResultAggregator.Handle(new TestExecutionMethodBeginClientEvent
                {
                    ClassName = "Class name test",
                    MethodName = "method name test",
                    NamespaceName = "namespace test",
                });
                TestResultAggregator.Handle(_testExecutionMethodPassedClientEvent);

                _passedResult =
                    TestResultAggregator
                    .CurrentReport
                    .TestResults.Where(w => w.ResultType == ResultType.Passed)
                    .Cast<TestCaseResult>()
                    .FirstOrDefault();
            }

            [Test]
            public void should_add_the_result_to_the_testReport()
            {
                TestResultAggregator.CurrentReport.TotalPassed.ShouldEqual(1);
            }

            [Test]
            public void Should_be_able_to_get_the_specific_passedResult()
            {
                _passedResult.ShouldNotBeNull();
            }

            [Test]
            public void Should_have_translated_the_ClassName()
            {
                _passedResult.ClassName.ShouldEqual(_testExecutionMethodPassedClientEvent.ClassName);
            }

            [Test]
            public void Should_have_translated_the_MethodName()
            {
                _passedResult.MethodName.ShouldEqual(_testExecutionMethodPassedClientEvent.MethodName);
            }
            [Test]
            public void Should_have_translated_the_NameSpace()
            {
                _passedResult.NamespaceName.ShouldEqual(_testExecutionMethodPassedClientEvent.NamespaceName);
            }
            [Test]
            public void Should_have_translated_the_Started()
            {
                _passedResult.Started.ShouldEqual(_testExecutionMethodPassedClientEvent.Started);
            }

            [Test]
            public void Should_not_have_an_ExceptionInfo()
            {
                _passedResult.ExceptionInfo.ShouldBeNull();
            }

            [Test]
            public void Should_have_translated_the_Finished()
            {
                _passedResult.Finished.ShouldEqual(_testExecutionMethodPassedClientEvent.Finished);
            }

            [Test]
            public void Should_have_a_final_result_of_a_failure()
            {
                TestResultAggregator.CurrentReport.FinalResult.ShouldEqual(RunCompletedState.Successful);
            }
        }

        [TestFixture]
        public class when_a_TestExecutionMethodFailedClientEvent_was_published : for_a_TestResultAggregator_that_should_handle_a_ClientEvent
        {
            private TestCaseResult _failedResult;
            private TestExecutionMethodFailedClientEvent _testExecutionMethodFailedClientEvent;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                _testExecutionMethodFailedClientEvent = new TestExecutionMethodFailedClientEvent()
                {
                    ClassName = "Class name test",
                    MethodName = "method name test",
                    NamespaceName = "namespace test",
                    Finished = new DateTime(2009, 1, 1, 1, 1, 1),
                    Started = new DateTime(2009, 1, 1, 1, 1, 2),
                    ExceptionInfo = new Exception("Hello world"),
                };

            }

            protected override void Because()
            {
                base.Because();
                TestResultAggregator.Handle(new TestExecutionMethodBeginClientEvent
                {
                    ClassName = "Class name test",
                    MethodName = "method name test",
                    NamespaceName = "namespace test",
                });
                TestResultAggregator.Handle(_testExecutionMethodFailedClientEvent);

                _failedResult =
                    TestResultAggregator
                    .CurrentReport
                    .TestResults.Where(w => w.ResultType == ResultType.Failed)
                    .Cast<TestCaseResult>()
                    .FirstOrDefault();
            }

            [Test]
            public void should_add_the_result_to_the_testReport()
            {
                TestResultAggregator.CurrentReport.TotalFailed.ShouldEqual(1);
            }

            [Test]
            public void Should_be_able_to_get_the_specific_passedResult()
            {
                _failedResult.ShouldNotBeNull();
            }

            [Test]
            public void Should_have_translated_the_ClassName()
            {
                _failedResult.ClassName.ShouldEqual(_testExecutionMethodFailedClientEvent.ClassName);
            }

            [Test]
            public void Should_have_translated_the_MethodName()
            {
                _failedResult.MethodName.ShouldEqual(_testExecutionMethodFailedClientEvent.MethodName);
            }
            [Test]
            public void Should_have_translated_the_NameSpace()
            {
                _failedResult.NamespaceName.ShouldEqual(_testExecutionMethodFailedClientEvent.NamespaceName);
            }
            [Test]
            public void Should_have_translated_the_Started()
            {
                _failedResult.Started.ShouldEqual(_testExecutionMethodFailedClientEvent.Started);
            }

            [Test]
            public void Should_not_have_an_ExceptionInfo()
            {
                _failedResult.ExceptionInfo.ShouldNotBeNull();
            }

            [Test]
            public void Should_have_translated_the_Finished()
            {
                _failedResult.Finished.ShouldEqual(_testExecutionMethodFailedClientEvent.Finished);
            }

            [Test]
            public void Should_be_a_failing_testCaseResult()
            {
                _failedResult.ResultType.ShouldEqual(ResultType.Failed);
            }

            [Test]
            public void Should_have_a_final_result_of_a_failure()
            {
                TestResultAggregator.CurrentReport.FinalResult.ShouldEqual(RunCompletedState.Failure);
            }
        }

        [TestFixture]
        public class when_a_BrowserHostCommunicationTimeout_has_been_published : for_a_TestResultAggregator_that_should_handle_a_ClientEvent
        {
            private BrowserHostCommunicationTimeoutServerEvent _browserHostCommunicationTimeoutServerEvent;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                _browserHostCommunicationTimeoutServerEvent = new BrowserHostCommunicationTimeoutServerEvent();
            }
            protected override void Because()
            {
                base.Because();

                TestResultAggregator.Handle(_browserHostCommunicationTimeoutServerEvent);
            }

            [Test]
            public void Should_add_a_failure_message_to_the_current_TestReport()
            {
                TestResultAggregator.CurrentReport.TotalFailed.ShouldEqual(1);
            }
        }

        [TestFixture]
        public class when_a_dialog_assertion_occurs_we_should_rePublish_failure_events : for_a_TestResultAggregator_that_should_handle_a_ClientEvent
        {
            private readonly List<TestCaseResult> _manufacturedFailedEvents = new List<TestCaseResult>();
            private TestCaseResult _manufacturedFailedEvent;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                TestEventSubscriptionManager
                    .AddListener<TestCaseResult>(e =>
                    {
                        if (e.ResultType == ResultType.SystemGeneratedFailure)
                        {
                            _manufacturedFailedEvents.Add(e);
                        }
                    });
            }

            protected override void Because()
            {
                base.Because();

                TestResultAggregator.Handle(new TestExecutionMethodBeginClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m0" });
                TestResultAggregator.Handle(new TestExecutionMethodPassedClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m0" });

                TestResultAggregator.Handle(new DialogAssertionServerEvent(DialogType.MessageBox) { Message = "some message here" });

                _manufacturedFailedEvent = _manufacturedFailedEvents.FirstOrDefault();
            }

            [Test]
            public void Should_have_manufactured_a_test_failed_event()
            {
                _manufacturedFailedEvent.ShouldNotBeNull();
            }

            [Test]
            public void should_only_have_published_one_failure()
            {
                _manufacturedFailedEvents.Count().ShouldEqual(1);
            }
        }

        [TestFixture]
        public class when_a_completed_MethodClientEvent_arrives_before_the_TestExecutionMethodBeginClientEvent : for_a_TestResultAggregator_that_should_handle_a_ClientEvent
        {
            [Test]
            public void Should_not_complete_a_Passed_event_until_the_begin_event_arrives()
            {
                TestResultAggregator.Handle(new TestExecutionMethodPassedClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m0" });
                TestResultAggregator.CurrentReport.TotalPassed.ShouldEqual(0);
                TestResultAggregator.Handle(new TestExecutionMethodBeginClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m0" });
                TestResultAggregator.CurrentReport.TotalPassed.ShouldEqual(1);
            }

            [Test]
            public void Should_not_complete_a_Failed_event_until_the_begin_event_arrives()
            {
                TestResultAggregator.Handle(new TestExecutionMethodFailedClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m1" });
                TestResultAggregator.CurrentReport.TotalPassed.ShouldEqual(0);
                TestResultAggregator.Handle(new TestExecutionMethodBeginClientEvent { NamespaceName = "n", ClassName = "c", MethodName = "m1" });
                TestResultAggregator.CurrentReport.TotalFailed.ShouldEqual(1);
            }

        }
    }
}
