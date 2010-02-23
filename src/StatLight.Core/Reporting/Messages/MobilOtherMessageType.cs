using System.Runtime.Serialization;
using System;

namespace StatLight.Core.Reporting.Messages
{
#if !SILVERLIGHT
	[DataContract]
	public class MobilOtherMessageType
	{
		[DataMember]
		public string Message { get; set; }

		[DataMember]
		public LogMessageType MessageType { get; set; }

		[DataMember]
		public int MessageOrder { get; set; }

		public string TraceMessage()
		{
			return @"
------------------ OtherMessage ------------------
MessageType: [{0}]
Message:
[{1}]
--------------------------------------------------
".FormatWith(MessageType,
																																												 Message);
		}


		public bool IsIgnoreMessage()
		{
			if (string.IsNullOrEmpty(this.Message))
				return false;

			if (this.MessageType == LogMessageType.TestExecution)
				if (this.Message.StartsWith("Ignoring", StringComparison.CurrentCulture))
					return true;

			return false;
		}

		public bool IsBrowserCommErrorMessage()
		{
			if (string.IsNullOrEmpty(this.Message))
				return false;

			if (this.MessageType == LogMessageType.Error)
				if (this.Message.Contains("communication"))
					if (this.Message.Contains(@"browser/xap"))
						return true;

			return false;
		}
	}
#endif
}