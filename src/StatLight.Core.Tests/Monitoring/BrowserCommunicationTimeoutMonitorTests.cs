using System;
using Moq;
using NUnit.Framework;
using StatLight.Core.Events;
using StatLight.Core.Monitoring;
using StatLight.Core.Timing;

namespace StatLight.Core.Tests.Monitoring
{
	namespace BrowserCommunicationTimeoutMonitorTests
	{
		public class with_a_BrowserCommunicationTimeoutMonitor : FixtureBase
		{
			public Mock<ITimer> MockTimer { get; private set; }

			public BrowserCommunicationTimeoutMonitor Monitor { get; private set; }

			protected override void Before_each_test()
			{
				base.Before_each_test();

				MockTimer = new Mock<ITimer>();

				Monitor = new BrowserCommunicationTimeoutMonitor(TestEventAggregator, MockTimer.Object,
																  new TimeSpan(0, 0, 0, 1, 0));
			}
		}

		[TestFixture]
		public class when_a_verifying_test_messages_arriving_from_the_xap_OUTSIDE_the_timeout_period : with_a_BrowserCommunicationTimeoutMonitor
		{

			private void PublishTimerFiredEvent()
			{
				MockTimer.Raise((t) => t.Elapsed += null, new TimerWrapperElapsedEventArgs(DateTime.Now.AddSeconds(2)));
			}

			[Test]
			public void Should_have_had_the_internal_timer_started()
			{
				MockTimer.Verify(v=>v.Start(), Times.Exactly(1));
			}

			[Test]
			public void Should_not_publish_events_more_than_once()
			{
				int publishedEventsCount = 0;
				base.TestEventAggregator
					.GetEvent<BrowserHostCommunicationTimeoutEvent>()
					.Subscribe((result) => { publishedEventsCount++; });

				PublishTimerFiredEvent();
				PublishTimerFiredEvent();

				publishedEventsCount.ShouldEqual(1);
			}

			[Test]
			public void Should_publish_browser_communication_timeout()
			{
				bool browserHostCommunicationTimeoutEventPublished = false;
				base.TestEventAggregator
					.GetEvent<BrowserHostCommunicationTimeoutEvent>()
					.Subscribe((result) => { browserHostCommunicationTimeoutEventPublished = true; });

				PublishTimerFiredEvent();

				browserHostCommunicationTimeoutEventPublished.ShouldBeTrue();
			}

			[Test]
			public void Should_publish_the_TestRunCompletedEvent()
			{
				bool testCompletedEventPublished = false;
				base.TestEventAggregator
					.GetEvent<TestRunCompletedEvent>()
					.Subscribe((result) => { testCompletedEventPublished = true; });

				PublishTimerFiredEvent();

				testCompletedEventPublished.ShouldBeTrue();
			}


			[Test]
			[Ignore]
			public void Should_close_the_browser_window()
			{
				Assert.Fail();
			}
		}

		[TestFixture]
		public class when_a_verifying_test_messages_arriving_from_the_xap_INSIDE_the_timeout_period : with_a_BrowserCommunicationTimeoutMonitor
		{
			private void PublishTimerFiredEvent()
			{
				MockTimer.Raise((t) => t.Elapsed += null, new TimerWrapperElapsedEventArgs(DateTime.Now));
			}

			[Test]
			public void Should_not_fire_the_comm_err_event_if_the_TestRunCompletedEvent_has_arrived()
			{
				bool wasEventPublished = false;
				base.TestEventAggregator
					.GetEvent<BrowserHostCommunicationTimeoutEvent>()
					.Subscribe((result) => { wasEventPublished = true; });

				base.TestEventAggregator
					.GetEvent<TestRunCompletedEvent>()
					.Publish();

				PublishTimerFiredEvent();

				wasEventPublished.ShouldBeFalse();
			}

			[Test]
			public void Should_not_publish_browser_communication_timeout_when_a_message_arrives_within_the_specified_timeout_period()
			{
				bool wasEventPublished = false;
				base.TestEventAggregator
					.GetEvent<TestResultEvent>()
					.Publish(null);

				base.TestEventAggregator
					.GetEvent<BrowserHostCommunicationTimeoutEvent>()
					.Subscribe((result) => { wasEventPublished = true; });

				PublishTimerFiredEvent();

				wasEventPublished.ShouldBeFalse();
			}
		}
	}
}