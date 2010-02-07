using System;
using StatLight.Client.Model.DTO;

namespace StatLight.Client.Model.Events
{
	public abstract class ClientEvent
	{
		public int MessageOrder { get; set; }
	}

	public abstract class TestResultCompletedEvent : ClientEvent
	{
		public ExecutedTestInfo ExecutedTestInfo { get; set; }
		public DateTime EndTime { get; set; }
	}





	public class TestBeginEvent : ClientEvent
	{
		public ExecutedTestInfo ExecutedTestInfo { get; set; }
		public DateTime StartTime { get; set; }
	}

	public class TestFailedEvent : TestResultCompletedEvent
	{
		public ExceptionInfo ExceptionInfo { get; set; }
	}

	public class TestPassedEvent : TestResultCompletedEvent
	{
	}
}