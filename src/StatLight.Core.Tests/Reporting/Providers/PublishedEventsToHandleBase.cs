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
            Handler.Handle(new TestExecutionMethodFailedClientEvent{ExceptionInfo = new Exception()});
        }


        [Test]
        public void Should_handle_the_TestExecutionMethodIgnoredClientEvent_event()
        {
            Handler.Handle(new TestExecutionMethodIgnoredClientEvent());
        }
    }
}