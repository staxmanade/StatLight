
namespace StatLight.Core.Tests.Events
{
	using NUnit.Framework;
	using StatLight.Core.Events;

	[TestFixture]
	public class TestResultEventTests
	{
		[Test]
		public void should_be_able_to_publish_an_event()
		{
			var x = new CompositeEvent<object>();
			x.Publish(null);
		}

		[Test]
		public void should_be_able_to_subscribe_to_event()
		{
			bool coughtEvent = false;
			var x = new CompositeEvent<object>();
			x.Subscribe((o) => { coughtEvent = true; });
			x.Publish(null);
			coughtEvent.ShouldBeTrue();
		}

	}
}
