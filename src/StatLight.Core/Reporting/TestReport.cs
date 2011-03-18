
namespace StatLight.Core.Reporting
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using StatLight.Core.Events;

    public class TestReport
    {
        private readonly string _xapPath;
        private readonly IList<TestCaseResult> _testCaseResults = new List<TestCaseResult>();

        public TestReport(string xapPath)
        {
            _xapPath = xapPath;
            DateTimeRunCompleted = DateTime.Now;
        }

        public string XapPath
        {
            get { return _xapPath; }
        }

        public IEnumerable<TestCaseResult> TestResults { get { return _testCaseResults; } }

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
                //TODO: this isn't a very accurate value considering other factors...

                return new TimeSpan(_testCaseResults
                    .Where(w => w is TestCaseResult)
                    .Cast<TestCaseResult>()
                    .Select(s => s.TimeToComplete.Ticks)
                    .Sum());
            }
        }

        public int TotalResults
        {
            get { return _testCaseResults.Count; }
        }

        public int TotalIgnored
        {
            get
            {
                var failures = _testCaseResults
                    .Where(w => w.ResultType == ResultType.Ignored);
                return failures.Count();
            }
        }

        public int TotalFailed
        {
            get
            {
                var failures = _testCaseResults
                    .Where(w => w.ResultType == ResultType.Failed || w.ResultType == ResultType.SystemGeneratedFailure);
                return failures.Count();
            }
        }

        public int TotalPassed
        {
            get
            {
                var failures = _testCaseResults
                    .Where(w => w.ResultType == ResultType.Passed);
                return failures.Count();
            }
        }

        public TestReport AddResult(TestCaseResult result)
        {
            SetLastMessageReceivedTime();
            _testCaseResults.Add(result);
            return this;
        }

        private void SetLastMessageReceivedTime()
        {
            DateTimeRunCompleted = DateTime.Now;
        }
    }
}
