using System;

namespace StatLight.Client.Model.Messaging
{
    public sealed class HttpResponseCompleteEventArgs : EventArgs
    {
        public string Response { get; private set; }
        public Exception Error { get; private set; }
        public HttpResponseCompleteEventArgs(string response)
        {
            Response = response;
        }
        public HttpResponseCompleteEventArgs(Exception exception)
        {
            Error = exception;
        }
    }
}