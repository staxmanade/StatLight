using NUnit.Framework;
using StatLight.Core.Events;
using StatLight.Client.Harness.Events;
using System;
using StatLight.Core.Reporting;

namespace StatLight.Core.Tests.Reporting.Providers
{

    public abstract class PublishedEventsToHandleBase<THandler> : FixtureBase
        where THandler : ITestingReportEvents
    {
        protected abstract THandler Handler { get; }

        private TestCaseResult Create(ResultType type)
        {
            return new TestCaseResult(type);
        }

        private TestCaseResult Create(ResultType type, ExceptionInfo ex)
        {
            return new TestCaseResult(type) { ExceptionInfo = ex };
        }

        [Test]
        public void Should_handle_the_TestExecutionMethodPassedClientEvent_event()
        {
            Handler.Handle(Create(ResultType.Passed));
        }

        [Test]
        public void Should_handle_the_TestExecutionMethodFailedClientEvent_event()
        {
            Exception exception;
            try
            {
                throw new Exception("Hello");
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            Handler.Handle(Create(ResultType.Failed, exception));
        }

        [Test]
        public void Should_handle_the_TestExecutionMethodIgnoredClientEvent_event()
        {
            Handler.Handle(Create(ResultType.Ignored));
        }

        [Test]
        public void Should_handle_the_TraceClientEvent()
        {
            Handler.Handle(new TraceClientEvent { Message = "Some trace message." });
        }

        [Test]
        public void Should_handle_the_DialogAssertionServerEvent()
        {
            Handler.Handle(Create(ResultType.SystemGeneratedFailure));
        }

        [Test]
        public void Should_handle_the_BrowserHostCommunicationTimeoutServerEvent()
        {
            Handler.Handle(new BrowserHostCommunicationTimeoutServerEvent { Message = "some timeout error message." });
        }

    }
}