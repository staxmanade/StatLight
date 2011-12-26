using Microsoft.Silverlight.Testing.Harness;
using StatLight.Core.Events;

namespace StatLight.Core.Events.Hosts.MSTest.LogMessagTranslation
{
    public interface ILogMessageToClientEventTranslation
    {
        bool CanTranslate(LogMessage message);
        ClientEvent Translate(LogMessage message);
    }
}