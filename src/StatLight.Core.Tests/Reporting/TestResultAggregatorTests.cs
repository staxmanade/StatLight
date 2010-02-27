
using System;
using System.Linq;
using NUnit.Framework;
using StatLight.Client.Harness.Events;
using StatLight.Core.Serialization;

namespace StatLight.Core.Tests.Reporting
{
    namespace TestResultAggregatorTests
    {
        using NUnit.Framework;
        using StatLight.Core.Events;
        using StatLight.Core.Reporting;
        using StatLight.Client.Harness.Events;

        public class for_a_TestResultAggregator : FixtureBase
        {
            public TestResultAggregator TestResultAggregator { get; private set; }

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                TestResultAggregator = new TestResultAggregator();
                TestEventAggregator.AddListener(TestResultAggregator);
            }
        }

        [TestFixture]
        public class when_a_TestExecutionMethodPassedClientEvent_was_published : for_a_TestResultAggregator
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

                TestEventAggregator
                    .SendMessage(_testExecutionMethodPassedClientEvent);

            }

            protected override void Because()
            {
                base.Because();
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
        public class when_a_TestExecutionMethodFailedClientEvent_was_published : for_a_TestResultAggregator
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

                TestEventAggregator
                    .SendMessage(_testExecutionMethodFailedClientEvent);

            }

            protected override void Because()
            {
                base.Because();
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
        public class when_a_BrowserHostCommunicationTimeout_has_been_published : for_a_TestResultAggregator
        {
            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                TestEventAggregator.SendMessage(new BrowserHostCommunicationTimeoutServerEvent());
            }

            //TODO:MobilOtherMessageType
            //[Test]
            //public void Should_add_a_failure_message_to_the_TestResultHandler()
            //{
            //    TestResultHandler
            //        .Verify(v => v.HandleMessage(
            //            It.Is<MobilOtherMessageType>(m => m.MessageType == LogMessageType.Error)));
            //}

            [Test]
            public void Should_add_a_failure_message_to_the_current_TestReport()
            {
                TestResultAggregator.CurrentReport.TotalFailed.ShouldEqual(1);
            }
        }
    }

    [TestFixture]
    public class When_serializing_and_deserializing_an_exceptionInfo : FixtureBase
    {
        Exception exception;
        private ExceptionInfo beforeExceptionInfo;
        private ExceptionInfo afterExceptionInfo;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            try
            {
                throw new Exception("Hello");
            }
            catch (Exception ex2)
            {
                exception = ex2;
            }

            beforeExceptionInfo = new ExceptionInfo(exception);

            afterExceptionInfo = beforeExceptionInfo.Serialize().Deserialize<ExceptionInfo>();

        }

        [Test]
        public void Should_have_FullMessage()
        {
            afterExceptionInfo.FullMessage.ShouldEqual(beforeExceptionInfo.FullMessage);
        }

        [Test]
        public void Should_have_Message()
        {
            afterExceptionInfo.Message.ShouldEqual(beforeExceptionInfo.Message);
        }

        [Test]
        public void Should_have_InnerException()
        {
            afterExceptionInfo.InnerException.ShouldEqual(beforeExceptionInfo.InnerException);
        }

        [Test]
        public void Should_have_StackTrace()
        {
            afterExceptionInfo.StackTrace.ShouldEqual(beforeExceptionInfo.StackTrace);
        }
    }
}
