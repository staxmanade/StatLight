using System;
using System.Diagnostics;
using StatLight.Client.Harness.Events;
using StatLight.Client.Model.Messaging;
using StatLight.Core.Serialization;
using StatLight.Core.WebServer;
using StatLight.Core.Configuration;

namespace StatLight.Client.Harness.Messaging
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

        [Conditional("DEBUG")]
        public static void Debug(string message)
        {
#if DEBUG
            //System.Console.WriteLine(message);
            var traceClientEvent = new DebugClientEvent
            {
                Message = message
            };
            PostMessage(traceClientEvent);
#endif
        }

        public static void LogException(Exception exception)
        {
#if WINDOWS_PHONE
            System.Windows.MessageBox.Show(exception.ToString());
#endif
            var messageObject = new UnhandledExceptionClientEvent
            {
                ExceptionInfo = exception,
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

        //private static Uri _postMessageUri;
        //public static void PostMessage(string message)
        //{
        //    System.Threading.Interlocked.Increment(ref _postMessageCount);

        //    if (_postMessageUri == null)
        //        _postMessageUri = StatLightServiceRestApi.PostMessage.ToFullUri();
        //    HttpPost(_postMessageUri, message);

        //}

        public static void PostMessage(string message)
        {
            System.Threading.Interlocked.Increment(ref _postMessageCount);

            HttpPost(StatLightServiceRestApi.PostMessage.ToFullUri(), message);
        }

        private static void HttpPost(Uri uri, string message)
        {
            // if the uri is null then there is no place to post (to support a "remotely hosted run" but not configured to run connnected to statlight
            if (uri == null)
                return;

            new HttpWebRequestHelper(uri, "POST", message).Execute();
        }
    }
}
