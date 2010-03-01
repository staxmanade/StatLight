
namespace StatLight.Core.Reporting
{
    using System.Collections.Generic;
    using System.Linq;
    using System;
    using StatLight.Client.Harness.Events;
    using System.Diagnostics;

    [DebuggerDisplay("Result=[{ResultType}], Method={NamespaceName}.{ClassName}.{MethodName}")]
    public class TestCaseResult
    {
        public TestCaseResult(ResultType resultType)
        {
            ResultType = resultType;
        }

        public string NamespaceName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Finished { get; set; }
        public string OtherInfo { get; set; }
        public TimeSpan TimeToComplete
        {
            get
            {
                if (Finished.HasValue)
                    return Finished.Value - Started;

                return new TimeSpan();
            }
        }
        public ExceptionInfo ExceptionInfo { get; set; }

        public ResultType ResultType { get; private set; }
    }

    public enum ResultType
    {
        Passed,
        Failed,
        Ignored,
        SystemGeneratedFailure,
    }


    public class TestReport
    {
        private readonly IList<TestCaseResult> _testCaseResults = new List<TestCaseResult>();

        public TestReport()
        {
            DateTimeRunCompleted = DateTime.Now;
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
                    .Where(w => w.ResultType == ResultType.Failed);
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

        //public TestReport AddResult(MobilOtherMessageType otherMessage)
        //{
        //    this.otherMessages.Add(otherMessage);
        //    DateTimeRunCompleted = DateTime.Now;
        //    return this;
        //}

        //public TestReport AddResult(MobilScenarioResult mobilScenarioResult)
        //{
        //    results.Add(mobilScenarioResult);
        //    DateTimeRunCompleted = DateTime.Now;
        //    return this;
        //}

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
