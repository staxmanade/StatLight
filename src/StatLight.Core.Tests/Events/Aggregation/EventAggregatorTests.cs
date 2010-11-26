using NUnit.Framework;
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Tests.Events.Aggregation
{

    namespace EventAggregatorTests
    {
        [TestFixture]
        public class when_adding_a_filtered_delegate_listener_to_the_EventAggregator : FixtureBase
        {
            private class TestEvent
            {
                public int Value { get; set; }
            }

            private IEventAggregator _eventAggregator;
            private bool _value1Handeled;
            private bool _value2NotHandeled;
            int _eventsSent;
            private int _notWantingEvents;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                _eventAggregator = new EventAggregator();

                _eventAggregator.AddListener<TestEvent>(o => _value1Handeled = true, e => e.Value == 1);
                _eventAggregator.AddListener<TestEvent>(o => _value2NotHandeled = true, e => e.Value != 1);

                _eventAggregator.AddListener(o => _eventsSent++, msg => msg.GetType().Name.EndsWith("Event"));

                _eventAggregator.AddListener(o => _notWantingEvents++, msg => false);

                _eventAggregator.SendMessage(new TestEvent { Value = 1 });
            }

            [Test]
            public void Should_have_found_filtered_handler()
            {
                _value1Handeled.ShouldBeTrue();
            }

            [Test]
            public void Should_not_handle_anything_when_the_filter_excludes_it()
            {
                _value2NotHandeled.ShouldBeFalse();
            }

            [Test]
            public void Should_publish_all_events_to_a_plain_object_listener()
            {
                _eventsSent.ShouldEqual(1);
            }

            [Test]
            public void Should_not_publish_anything_to_non_needed_listner()
            {
                _notWantingEvents.ShouldEqual(0);
            }
        }

        [TestFixture]
        public class when_giving_the_event_aggregator_a_class_that_listens_to_multiple_events : FixtureBase
        {
            private IEventAggregator _eventAggregator;

            private static bool _intListenerHandledMessage;
            private static bool _stringListenerHandledMessage;

            protected override void Before_all_tests()
            {
                base.Before_all_tests();

                _eventAggregator = new EventAggregator();
                _eventAggregator.AddListener(new MultiListener());
            }

            protected override void Because()
            {
                base.Because();

                _eventAggregator.SendMessage(1);
                _eventAggregator.SendMessage("hello");
            }

            [Test]
            public void Should_have_heard_int_event()
            {
                _intListenerHandledMessage.ShouldBeTrue();
            }

            [Test]
            public void Should_have_heard_string_event()
            {
                _stringListenerHandledMessage.ShouldBeTrue();
            }

            private class MultiListener : IListener<int>, IListener<string>
            {
                public void Handle(int message)
                {
                    _intListenerHandledMessage = true;
                }

                public void Handle(string message)
                {
                    _stringListenerHandledMessage = true;
                }
            }
        }
    }
}