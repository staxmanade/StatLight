using System;
using Microsoft.Silverlight.Testing.Harness;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public class InitializationOfUnitTestHarnessClientEventMap
        : ILogMessageToClientEventTranslation
    {
        public bool CanTranslate(LogMessage message)
        {
            if (message.MessageType == LogMessageType.TestInfrastructure)
            {
                if (message.Message.Equals("Initialization of UnitTestHarness", StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public ClientEvent Translate(LogMessage message)
        {
            var clientEventX = new InitializationOfUnitTestHarnessClientEvent { };
            return clientEventX;
        }
    }
}