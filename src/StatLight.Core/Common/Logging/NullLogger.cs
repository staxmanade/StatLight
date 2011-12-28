namespace StatLight.Core.Common.Logging
{
	public sealed class NullLogger : LoggerBase
	{
		public NullLogger()
			: base(LogChatterLevels.None)
		{
		}

		public override void Information(string message)
		{
		}

		public override void Debug(string message)
		{
		}

		public override void Debug(string message, bool writeNewLine)
		{
		}

		public override void Warning(string message)
		{
		}

		public override void Error(string message)
		{
		}
	}

}
