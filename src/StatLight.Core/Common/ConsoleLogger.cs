
using System;
using StatLight.Core.Properties;

namespace StatLight.Core.Common
{
    public class ConsoleLogger : LoggerBase
    {
        private readonly Settings _settings;

        public ConsoleLogger()
            : this(LogChatterLevels.Error | LogChatterLevels.Warning | LogChatterLevels.Information, Settings.Default)
        {
        }

        public ConsoleLogger(LogChatterLevels logChatterLevel)
            : this(logChatterLevel, Settings.Default)
        {
        }

        public ConsoleLogger(LogChatterLevels logChatterLevel, Settings settings)
            : base(logChatterLevel)
        {
            _settings = settings;
        }

        public override void Information(string message)
        {
            if (ShouldLog(LogChatterLevels.Information))
                WrapMessageWithColor(message, _settings.ConsoleColorInformatoin, false);
        }

        public override void Debug(string message)
        {
            if (ShouldLog(LogChatterLevels.Debug))
                WrapMessageWithColor(message, _settings.ConsoleColorDebug, true);
        }

        public override void Debug(string message, bool writeNewLine)
        {
            if (ShouldLog(LogChatterLevels.Debug))
                WrapMessageWithColor(message, _settings.ConsoleColorDebug, writeNewLine);
        }

        public override void Warning(string message)
        {
            if (ShouldLog(LogChatterLevels.Warning))
                WrapMessageWithColor(message, _settings.ConsoleColorWarning, false);
        }

        public override void Error(string message)
        {
            if (ShouldLog(LogChatterLevels.Error))
                WrapMessageWithColor(message, _settings.ConsoleColorError, true);
        }


        private static void WrapMessageWithColor(string message, ConsoleColor color, bool useNewLine)
        {
            message.WrapConsoleMessageWithColor(color, useNewLine);
        }

    }

}
