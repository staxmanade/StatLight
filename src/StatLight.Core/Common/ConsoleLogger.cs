
using System;
namespace StatLight.Core.Common
{
	public class ConsoleLogger : LoggerBase
	{
		public ConsoleLogger()
			: this(LogChatterLevels.Error | LogChatterLevels.Warning | LogChatterLevels.Information)
		{
		}

		public ConsoleLogger(LogChatterLevels logChatterLevel)
			: base(logChatterLevel)
		{
		}

		public override void Information(string message)
		{
			if (ShouldLog(LogChatterLevels.Information))
				WrapMessageWithColor(message, ConsoleColor.White, false);
		}

		public override void Debug(string message)
		{
			if (ShouldLog(LogChatterLevels.Debug))
				WrapMessageWithColor(message, ConsoleColor.Cyan, true);
		}

		public override void Debug(string message, bool writeNewLine)
		{
			if (ShouldLog(LogChatterLevels.Debug))
				WrapMessageWithColor(message, ConsoleColor.Cyan, writeNewLine);
		}

		public override void Warning(string message)
		{
			if (ShouldLog(LogChatterLevels.Warning))
				WrapMessageWithColor(message, ConsoleColor.Yellow, false);
		}

		public override void Error(string message)
		{
			if (ShouldLog(LogChatterLevels.Error))
				WrapMessageWithColor(message, ConsoleColor.Red, true);
		}


		private static void WrapMessageWithColor(string message, ConsoleColor color, bool useNewLine)
		{
			message.WrapConsoleMessageWithColor(color, useNewLine);
		}

	}

}
