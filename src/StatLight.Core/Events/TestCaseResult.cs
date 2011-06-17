using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using StatLight.Client.Harness.Events;

namespace StatLight.Core.Events
{
    [DebuggerDisplay("Result=[{ResultType}], Method={NamespaceName}.{ClassName}.{MethodName}")]
    public class TestCaseResult
    {
        private readonly List<MetaDataInfo> _metadata;

        public TestCaseResult(ResultType resultType)
        {
            ResultType = resultType;
            _metadata = new List<MetaDataInfo>();
        }

        public string NamespaceName { get; set; }
        public string ClassName { get; set; }
        public string MethodName { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Finished { get; set; }
        public string OtherInfo { get; set; }

        public void PopulateMetadata(IEnumerable<MetaDataInfo> metadata)
        {
            if (metadata == null) throw new ArgumentNullException("metadata");
            _metadata.AddRange(metadata);
        }

        public IEnumerable<MetaDataInfo> Metadata { get { return _metadata; } }

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