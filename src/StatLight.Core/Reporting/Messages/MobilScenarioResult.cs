using System;
using System.Runtime.Serialization;

namespace StatLight.Core.Reporting.Messages
{
#if !SILVERLIGHT
	[DataContract]
	public class MobilScenarioResult
	{
		[DataMember]
		public string TestName { get; set; }

		[DataMember]
		public string TestClassName { get; set; }

		public TimeSpan TimeToComplete { get { return this.Finished - this.Started; } }

		private string exceptionMessage;

		[DataMember]
		public string ExceptionMessage
		{
			get { return this.exceptionMessage; }
			set
			{
				if (!string.IsNullOrEmpty(value))
					this.exceptionMessage = value.Replace("&lt;", "<").Replace("&gt;", ">");
				else
					this.exceptionMessage = value;
			}
		}

		[DataMember]
		public DateTime Finished { get; set; }

		[DataMember]
		public TestOutcome Result { get; set; }

		[DataMember]
		public DateTime Started { get; set; }

		[DataMember]
		public int MessageOrder { get; set; }

		public string TraceMessage()
		{
			return @"
Test Class: {0}
Test Method: {1}
Exception Message:
{2}
".FormatWith(TestClassName,
																								TestName,
																								ExceptionMessage
				);
		}
	}
#endif
}