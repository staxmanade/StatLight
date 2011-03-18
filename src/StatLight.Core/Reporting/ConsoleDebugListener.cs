using StatLight.Client.Harness.Events;
using StatLight.Core.Common;
using StatLight.Core.Events.Aggregation;

namespace StatLight.Core.Reporting
{
    public class ConsoleDebugListener : IListener<DebugClientEvent>
    {
        private readonly ILogger _logger;

        public ConsoleDebugListener(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(DebugClientEvent message)
        {
            _logger.Debug(message.Message);
        }
    }
}