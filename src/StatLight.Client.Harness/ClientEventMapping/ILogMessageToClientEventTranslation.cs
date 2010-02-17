using Microsoft.Silverlight.Testing.Harness;
using StatLight.Client.Model.Events;

namespace StatLight.Client.Harness.ClientEventMapping
{
    public interface ILogMessageToClientEventTranslation
    {
        bool CanTranslate(LogMessage message);
        ClientEvent Translate(LogMessage message);
    }
}