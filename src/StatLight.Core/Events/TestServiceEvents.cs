
namespace StatLight.Core.Events
{
	using StatLight.Core.Reporting.Messages;
using System;

	public sealed class TestResultEvent : CompositeEvent<MobilScenarioResult> { }
	public sealed class TestHarnessOtherMessageEvent : CompositeEvent<MobilOtherMessageType> { }

	public sealed class DialogAssertionEvent : CompositeEvent<MobilScenarioResult> { }

	public sealed class EmptyPayload
	{
		private EmptyPayload()
		{
		}

		public static EmptyPayload New
		{
			get
			{
				return new EmptyPayload();
			}
		}
	}
	public class EmptyPayloadCompositeEvent : CompositeEvent<EmptyPayload>
	{
		public void Publish()
		{
			base.Publish(EmptyPayload.New);
		}
	}

	public sealed class TestRunCompletedEvent : EmptyPayloadCompositeEvent
	{
	}

	public sealed class BrowserHostCommunicationTimeoutEvent : EmptyPayloadCompositeEvent
	{
	}
}
