
namespace StatLight.Core.Reporting
{
	using System.Linq;
	using System;
	using System.Collections.ObjectModel;
	using StatLight.Core.Reporting.Messages;

	public class TestReport
	{
		public TestReport()
		{
			this.DateTimeRunCompleted = DateTime.Now;
		}

		private Collection<MobilScenarioResult> results = new Collection<MobilScenarioResult>();
		private Collection<MobilOtherMessageType> otherMessages = new Collection<MobilOtherMessageType>();

		public Collection<MobilScenarioResult> Results { get { return results; } }
		public Collection<MobilOtherMessageType> OtherMessages { get { return otherMessages; } }

		public DateTime DateTimeRunCompleted { get; private set; }

		public RunCompletedState FinalResult
		{
			get
			{
				if (TotalFailed > 0)
					return RunCompletedState.Failure;

				return RunCompletedState.Successful;
			}
		}

		public TimeSpan TimeToComplete
		{
			get
			{
				return
					new TimeSpan(results
						.Select(s => s.Finished - s.Started)
						.Sum(x => x.Ticks));
			}
		}

		public int TotalResults
		{
			get { return results.Count + TotalIgnored; }
		}

		public int TotalIgnored
		{
			get
			{
				return otherMessages
						.Where(w => w.IsIgnoreMessage())
						.Count();
			}
		}

		public int TotalFailed
		{
			get
			{
				var failures = results.Where(w => w.Result == TestOutcome.Failed || w.Result == TestOutcome.Inconclusive).Count();
				return failures;
			}
		}

		public int TotalPassed
		{
			get { return results.Where(w => w.Result == TestOutcome.Passed).Count(); }
		}

		public TestReport AddResult(MobilOtherMessageType otherMessage)
		{
			this.otherMessages.Add(otherMessage);
			DateTimeRunCompleted = DateTime.Now;
			return this;
		}

		public TestReport AddResult(MobilScenarioResult mobilScenarioResult)
		{
			results.Add(mobilScenarioResult);
			DateTimeRunCompleted = DateTime.Now;
			return this;
		}
	}
}
