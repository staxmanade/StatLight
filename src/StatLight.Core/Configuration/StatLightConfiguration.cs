using System;
using StatLight.Core.WebServer;

namespace StatLight.Core.Configuration
{
    public class StatLightConfiguration
    {
        public ClientTestRunConfiguration Client { get; private set; }
        public ServerTestRunConfiguration Server { get; private set; }

        public StatLightConfiguration(ClientTestRunConfiguration clientConfig, ServerTestRunConfiguration serverConfig)
        {
            if (clientConfig == null) throw new ArgumentNullException("clientConfig");
            if (serverConfig == null) throw new ArgumentNullException("serverConfig");

            Client = clientConfig;
            Server = serverConfig;
        }

    }
}