using System;
using StatLight.Client.Harness.Events;
using StatLight.Client.Model.Messaging;
using StatLight.Core.Serialization;
using StatLight.Core.WebServer;

namespace StatLight.Client.Harness
{
    public class Server
    {
        public static void Trace(string message)
        {
            if (string.IsNullOrEmpty(message))
                message = "{StatLight - Trace Message: trace string is NULL or empty}";
            var traceClientEvent = new TraceClientEvent { Message = message };
            PostMessage(traceClientEvent);
        }

        public static void Debug(string message)
        {
            if (string.IsNullOrEmpty(message))
                message = "{StatLight - Debug Message: trace string is NULL or empty}";
            var traceClientEvent = new DebugClientEvent { Message = message };
            PostMessage(traceClientEvent);
        }

        public static void PostMessage(string message)
        {
            PostMessageX(message);
        }

        public static void PostMessage(object message)
        {
            string traceMessage = message.Serialize();
            PostMessageX(traceMessage);
        }

        public static void PostMessage(ClientEvent message)
        {
            string traceMessage = message.Serialize();
            PostMessageX(traceMessage);
        }


        /// <summary>
        /// Send a message back to the server signaling that all the tests have completed.
        /// </summary>
        public static void SignalTestComplete()
        {
            PostMessage(new SignalTestCompleteClientEvent { TotalMessagesPostedCount = postMessageCount });
        }



        private static int postMessageCount;
        private static void PostMessageX(string message)
        {
            postMessageCount++;

            HttpPost(StatLightServiceRestApi.PostMessage.ToFullUri(), message);
        }

        private static void HttpPost(Uri uri, string message)
        {
            new HttpWebRequestHelper(uri, "POST", message).Execute();
        }
    }
}