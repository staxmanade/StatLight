using System;
using System.Reflection;
using StatLight.Client.Harness.Events;
using StatLight.Client.Model.Messaging;
using StatLight.Core.Serialization;
using StatLight.Core.WebServer;
using StatLight.Core.Configuration;

namespace StatLight.Client.Harness
{
    public class Server
    {
        private static int _postMessageCount;

        public static void Trace(string message)
        {
            if (string.IsNullOrEmpty(message))
                message = "{StatLight - Trace Message: trace string is NULL or empty}";
            var traceClientEvent = new TraceClientEvent
            {
                Message = message
            };

            PostMessage(traceClientEvent);
        }

        public static void Debug(string message)
        {
#if DEBUG
            var traceClientEvent = new DebugClientEvent
            {
                Message = message
            };
            PostMessage(traceClientEvent);
#endif
        }

        public static void LogException(Exception exception)
        {
            var messageObject = new UnhandledExceptionClientEvent
            {
                Exception = exception,
            };

            PostMessage(messageObject);
        }

        public static void SignalTestComplete(SignalTestCompleteClientEvent signalTestCompleteClientEvent)
        {
            signalTestCompleteClientEvent.TotalMessagesPostedCount = _postMessageCount + 1;
            signalTestCompleteClientEvent.BrowserInstanceId = ClientTestRunConfiguration.InstanceNumber;
            PostMessage(signalTestCompleteClientEvent);
        }

        public static void PostMessage(ClientEvent message)
        {
            string messageString = message.Serialize();
            PostMessage(messageString);
        }

        public static void PostMessage(string message)
        {
            System.Threading.Interlocked.Increment(ref _postMessageCount);

            HttpPost(StatLightServiceRestApi.PostMessage.ToFullUri(), message);
        }

        private static void HttpPost(Uri uri, string message)
        {
            new HttpWebRequestHelper(uri, "POST", message).Execute();
        }
    }
}
