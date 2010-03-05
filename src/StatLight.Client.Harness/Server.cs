using System;
using Microsoft.Silverlight.Testing.Harness;
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

        #region SignalTestComplate


        /// <summary>
        /// Send a message back to the server signaling that all the tests have completed.
        /// </summary>
        /// <param name="state"></param>
        public static void SignalTestComplete(TestHarnessState state)
        {
            var signalTestCompleteClientEvent = new SignalTestCompleteClientEvent
            {
                TotalMessagesPostedCount = postMessageCount,
                Failed = state.Failed,
                TotalFailureCount = state.Failures,
                TotalTestsCount = state.TotalScenarios,
            };
            SignalTestComplate(signalTestCompleteClientEvent);
        }

        public static void SignalTestComplete()
        {
            SignalTestComplate(new SignalTestCompleteClientEvent { TotalMessagesPostedCount = postMessageCount });
        }

        private static void SignalTestComplate(SignalTestCompleteClientEvent completeClientEvent)
        {
            PostMessage(completeClientEvent);
        }


        #endregion
    }
}