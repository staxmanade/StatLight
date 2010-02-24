using NUnit.Framework;
using StatLight.Core.Events.Aggregation;
using StatLight.Client.Harness.Events;
using StatLight.Core.Serialization;
using System;

namespace StatLight.Core.Tests.Reporting.Providers
{

    public abstract class PublishedEventsToHandleBase<THandler> : FixtureBase
        where THandler : IListener<TestExecutionMethodPassedClientEvent>,
                         IListener<TestExecutionMethodFailedClientEvent>,
                         IListener<TestExecutionMethodIgnoredClientEvent>
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