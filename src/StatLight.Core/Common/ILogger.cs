
using StatLight.Core.Common.Logging;

namespace StatLight.Core.Common
{
    public interface ILogger
    {
        void Information(string message);
        void Warning(string message);
        void Error(string message);
        void Debug(string message);
        void Debug(string message, bool writeNewLine);

        LogChatterLevels LogChatterLevel { get; set; }
    }
}
