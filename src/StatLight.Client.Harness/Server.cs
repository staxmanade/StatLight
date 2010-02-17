using StatLight.Client.Model.Events;
using StatLight.Core.Serialization;

namespace StatLight.Client.Harness
{
    public static class Server
    {
        public static void Trace(string message)
        {
            if (string.IsNullOrEmpty(message))
                message = "{StatLight - Trace Message: trace string is NULL or empty}";
            var traceClientEvent = new TraceClientEvent { Message = message };
            string traceMessage = traceClientEvent.Serialize();
            StatLightPostbackManager.PostMessage(traceMessage);
        }
    }
}