
namespace StatLight.Core.Common.Logging
{
	public abstract class LoggerBase : ILogger
	{
		protected LoggerBase(LogChatterLevels logChatterLevel)
		{
			this.LogChatterLevel = logChatterLevel;
		}

		public abstract void Information(string message);
		public abstract void Warning(string message);
		public abstract void Error(string message);
		public abstract void Debug(string message);
		public abstract void Debug(string message, bool writeNewLine);
		public LogChatterLevels LogChatterLevel { get; set; }

		public bool ShouldLog(LogChatterLevels checkWith)
		{
			int current = (int)LogChatterLevel;
			int test = (int)(LogChatterLevel | checkWith);

			return test == current;
		}
	}
}
