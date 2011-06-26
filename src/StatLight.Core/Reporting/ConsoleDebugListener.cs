using System;
using StatLight.Client.Harness.Events;
using StatLight.Core.Common;
using EventAggregatorNet;

namespace StatLight.Core.Reporting
{
    internal class ConsoleDebugListener : IListener<DebugClientEvent>
    {
        private readonly ILogger _logger;

        public ConsoleDebugListener(ILogger logger)
        {
            _logger = logger;
        }

        public void Handle(DebugClientEvent message)
        {
            if (message == null) throw new ArgumentNullException("message");
            _logger.Debug(message.Message);
        }
    }
}