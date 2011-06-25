using System;
using Moq;
using NUnit.Framework;
using StatLight.Core.Common.Abstractions.Timing;
using StatLight.Core.Events;
using StatLight.Core.Monitoring;
using EventAggregatorNet;

namespace StatLight.Core.Tests.Monitoring
{
    namespace BrowserCommunicationTimeoutMonitorTests
    {
        public class with_a_BrowserCommunicationTimeoutMonitor : FixtureBase
        {
            public Mock<ITimer> MockTimer { get; private set; }

            public BrowserCommunicationTimeoutMonitor Monitor { get; private set; }

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                MockTimer = new Mock<ITimer>();

                Monitor = new BrowserCommunicationTimeoutMonitor(TestEventPublisher, MockTimer.Object,
                                                                  new TimeSpan(0, 0, 0, 1, 0));
            }

            protected virtual void PublishTimerFiredEvent()
            {
                MockTimer.Raise(t => t.Elapsed += null, new TimerWrapperElapsedEventArgs(DateTime.Now.AddSeconds(2)));
            }

            protected bool EventPublished;

            public void SetupEventToSeeIfPublished()
            {
                TestEventSubscriptionManager
                    .AddListener<BrowserHostCommunicationTimeoutServerEvent>(e => EventPublished = true);
            }
        }

        [TestFixture]
        public class for_a_BrowserHostCommunicationTimeoutEvent_xX :
            when_a_verifying_test_messages_arriving_from_the_xap_INSIDE_the_timeout_period
        {
            protected override void Before_all_tests()
            {
                base.Before_all_tests();
                SetupEventToSeeIfPublished();
                TestEventPublisher.SendMessage(new TestRunCompletedServerEvent());
            }


            [Test]
            public void Should_not_fire_the_comm_err_event_if_the_TestRunCompletedEvent_has_arrived()
            {
                EventPublished.ShouldBeFalse();
            }
        }


        [TestFixture]
        public class for_a_BrowserHostCommunicationTimeoutEvent_x :
            when_a_verifying_test_messages_arriving_from_the_xap_INSIDE_the_timeout_period
        {
            protected override void Before_all_tests()
            {
                base.Before_all_tests();
                //TestEventPublisher.SendMessage(new TestResultEvent());
                SetupEventToSeeIfPublished();
            }


            [Test]
            public void Should_not_publish_browser_communication_timeout_when_a_message_arrives_within_the_specified_timeout_period()
            {
                EventPublished.ShouldBeFalse();
            }
        }

        public class when_a_verifying_test_messages_arriving_from_the_xap_INSIDE_the_timeout_period
            : with_a_BrowserCommunicationTimeoutMonitor
        {
            protected override void PublishTimerFiredEvent()
            {
                MockTimer.Raise(t => t.Elapsed += null, new TimerWrapperElapsedEventArgs(DateTime.Now));
            }

            protected override void Because()
            {
                base.Because();
                PublishTimerFiredEvent();
            }
        }

        #region when_a_verifying_test_messages_arriving_from_the_xap_OUTSIDE_the_timeout_period
        public class when_a_verifying_test_messages_arriving_from_the_xap_OUTSIDE_the_timeout_period
            : with_a_BrowserCommunicationTimeoutMonitor
        {
            protected override void Because()
            {
                base.Because();
                PublishTimerFiredEvent();
            }
        }

        [TestFixture]
        public class and_an_event_was_published_more_than_once :
            when_a_verifying_test_messages_arriving_from_the_xap_OUTSIDE_the_timeout_period
        {
            private int _publishedEventsCount;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                TestEventSubscriptionManager
                    .AddListener<BrowserHostCommunicationTimeoutServerEvent>(o => _publishedEventsCount++);
            }

            protected override void Because()
            {
                base.Because();
                PublishTimerFiredEvent();
                PublishTimerFiredEvent();
            }

            [Test]
            public void Should_not_publish_events_more_than_once()
            {

                _publishedEventsCount.ShouldEqual(1);
            }
        }


        [TestFixture]
        public class for_a_BrowserHostCommunicationTimeoutEvent :
            when_a_verifying_test_messages_arriving_from_the_xap_OUTSIDE_the_timeout_period
        {
            protected override void Before_all_tests()
            {
                base.Before_all_tests();
                SetupEventToSeeIfPublished();
            }

            [Test]
            public void Should_publish_browser_communication_timeout()
            {
                EventPublished.ShouldBeTrue();
            }
        }


        [TestFixture]
        public class for_a_TestRunCompletedEvent :
            when_a_verifying_test_messages_arriving_from_the_xap_OUTSIDE_the_timeout_period
        {
            protected override void Before_all_tests()
            {
                base.Before_all_tests();
                SetupEventToSeeIfPublished();
            }

            [Test]
            public void Should_publish_browser_communication_timeout()
            {
                EventPublished.ShouldBeTrue();
            }
        }

        [TestFixture]
        public class stub_test
            : when_a_verifying_test_messages_arriving_from_the_xap_OUTSIDE_the_timeout_period
        {

            [Test]
            public void Should_have_had_the_internal_timer_started()
            {
                MockTimer.Verify(v => v.Start(), Times.Exactly(1));
            }

            [Test]
            [Ignore]
            public void Should_close_the_browser_window()
            {
                Assert.Fail();
            }
        }

        #endregion
    }
}