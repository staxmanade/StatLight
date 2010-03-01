using NUnit.Framework;
using StatLight.Core.Events;
using StatLight.Client.Harness.Events;
using System;

namespace StatLight.Core.Tests.Reporting.Providers
{

    public abstract class PublishedEventsToHandleBase<THandler> : FixtureBase
        where THandler : ITestingReportEvents
    {
        protected abstract THandler Handler { get; }

        [Test]
        public void Should_handle_the_TestExecutionMethodPassedClientEvent_event()
        {
            Handler.Handle(new TestExecutionMethodPassedClientEvent());
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
            Handler.Handle(new TestExecutionMethodFailedClientEvent { ExceptionInfo = exception });
        }

        [Test]
        public void Should_handle_the_TestExecutionMethodIgnoredClientEvent_event()
        {
            Handler.Handle(new TestExecutionMethodIgnoredClientEvent());
        }

        [Test]
        public void Should_handle_the_TraceClientEvent()
        {
            Handler.Handle(new TraceClientEvent { Message = "Some trace message." });
        }

        [Test]
        public void Should_handle_the_DialogAssertionServerEvent()
        {
            Handler.Handle(new DialogAssertionServerEvent { Message = "dialog found error" });
        }

        [Test]
        public void Should_handle_the_BrowserHostCommunicationTimeoutServerEvent()
        {
            Handler.Handle(new BrowserHostCommunicationTimeoutServerEvent { Message = "some timeout error message." });
        }

    }
}