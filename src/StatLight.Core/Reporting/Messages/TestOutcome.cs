namespace StatLight.Core.Reporting.Messages
{
	public enum TestOutcome
	{
		Error = 0,
		Failed = 1,
		Timeout = 2,
		Aborted = 3,
		Inconclusive = 4,
		PassedButRunAborted = 5,
		NotRunnable = 6,
		NotExecuted = 7,
		Disconnected = 8,
		Warning = 9,
		Passed = 10,
		Completed = 11,
		InProgress = 12,
		Pending = 13,
	}
}