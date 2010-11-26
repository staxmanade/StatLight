using NUnit.Framework;
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Tests.Events.Aggregation
{

    namespace EventAggregatorTests
    {
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