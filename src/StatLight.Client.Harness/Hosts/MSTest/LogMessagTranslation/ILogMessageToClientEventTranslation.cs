using Microsoft.Silverlight.Testing.Harness;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public interface ILogMessageToClientEventTranslation
    {
        bool CanTranslate(LogMessage message);
        ClientEvent Translate(LogMessage message);
    }
}