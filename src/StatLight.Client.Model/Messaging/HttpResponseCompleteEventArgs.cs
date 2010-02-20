using System;

namespace StatLight.Client.Model.Messaging
{
    public sealed class HttpResponseCompleteEventArgs : EventArgs
    {
        public string Response { get; set; }

        public HttpResponseCompleteEventArgs(string response)
        {
            this.Response = response;
        }
    }
}