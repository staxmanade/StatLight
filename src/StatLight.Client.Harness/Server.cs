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
#if DEBUG
            var traceClientEvent = new DebugClientEvent { Message = message };
            PostMessage(traceClientEvent);
#endif
        }

        public static void PostMessage(string message)
        {
            PostMessageX(message);
        }


        public static void PostMessage(ClientEvent message)
        {
            string traceMessage = message.Serialize();
            PostMessageX(traceMessage);
        }

        private static int _postMessageCount;
        private static void PostMessageX(string message)
        {
            System.Threading.Interlocked.Increment(ref _postMessageCount);

            HttpPost(StatLightServiceRestApi.PostMessage.ToFullUri(), message);
        }

        private static void HttpPost(Uri uri, string message)
        {
            new HttpWebRequestHelper(uri, "POST", message).Execute();
        }

        #region SignalTestComplate

        /// <summary>
        /// Send a message back to the server signaling that all the tests have completed.
        /// </summary>
        public static void SignalTestComplete(SignalTestCompleteClientEvent signalTestCompleteClientEvent)
        {
            signalTestCompleteClientEvent.TotalMessagesPostedCount = _postMessageCount;

            SignalTestComplate(signalTestCompleteClientEvent);
        }

        public static void SignalTestComplete()
        {
            SignalTestComplate(new SignalTestCompleteClientEvent { TotalMessagesPostedCount = _postMessageCount });
        }

        private static void SignalTestComplate(SignalTestCompleteClientEvent completeClientEvent)
        {
            PostMessage(completeClientEvent);
        }


        #endregion
    }
}
