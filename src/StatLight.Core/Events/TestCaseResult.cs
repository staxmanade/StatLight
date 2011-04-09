using System;
using System.Diagnostics;
using StatLight.Client.Harness.Events;
using StatLight.Core.Reporting;

namespace StatLight.Core.Events
{
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

        public string FullMethodName()
        {
            const string delimiter = ".";
            return (NamespaceName ?? string.Empty) + delimiter +
                   (ClassName ?? string.Empty) + delimiter +
                   (MethodName ?? string.Empty);
        }
    }
}