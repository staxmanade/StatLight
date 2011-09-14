using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using StatLight.Core.Events;

namespace StatLight.Core.Reporting
{
    public class TestReportCollection : IEnumerable<TestReport>
    {
        private readonly List<TestReport> _testReports = new List<TestReport>();

        public RunCompletedState FinalResult
        {
            get
            {
                if (_testReports.Any(testReport => testReport.FinalResult == RunCompletedState.Failure))
                {
                    return RunCompletedState.Failure;
                }

                return RunCompletedState.Successful;
            }
        }

        public int TotalResults { get { return _testReports.Sum(s => s.TotalResults); } }
        public int TotalPassed { get { return _testReports.Sum(s => s.TotalPassed); } }
        public int TotalIgnored { get { return _testReports.Sum(s => s.TotalIgnored); } }
        public int TotalFailed { get { return _testReports.Sum(s => s.TotalFailed); } }

        public DateTime DateTimeRunCompleted { get { return _testReports.Max(m => m.DateTimeRunCompleted); } }

        public IEnumerator<TestReport> GetEnumerator()
        {
            return _testReports.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TestReport testReport)
        {
            _testReports.Add(testReport);
        }

        public IEnumerable<TestCaseResult> AllTests()
        {
            return this.SelectMany(s => s.TestResults);
        }
    }
}