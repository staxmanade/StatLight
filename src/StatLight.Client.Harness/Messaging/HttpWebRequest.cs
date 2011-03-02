using System;
using System.IO;
using System.Net;

namespace StatLight.Client.Model.Messaging
{
    public class HttpWebRequestHelper
    {
        private WebRequest Request { get; set; }
        public string PostData { get; private set; }

        public event HttpResponseCompleteEventHandler ResponseComplete;

        private void OnResponseComplete(HttpResponseCompleteEventArgs e)
        {
            var handler = ResponseComplete;
            if (handler != null)
            {
                handler(e);
            }
        }

        public HttpWebRequestHelper(Uri requestUri, string method, string postData)
        {
            Request = WebRequest.Create(requestUri);
            Request.ContentType = "application/x-www-form-urlencoded";
            Request.Method = method;
            PostData = postData;
        }

        public void Execute()
        {
            Request.BeginGetRequestStream(BeginRequest, this);
        }

        private void BeginRequest(IAsyncResult ar)
        {
            var helper = ar.AsyncState as HttpWebRequestHelper;
            if (helper != null)
            {
                if (!string.IsNullOrEmpty(PostData))
                {
                    using (var writer = new StreamWriter(helper.Request.EndGetRequestStream(ar)))
                    {
                        writer.Write(Uri.EscapeDataString(PostData));
                    }
                }

                helper.Request.BeginGetResponse(BeginResponse, helper);
            }
        }

        private static void BeginResponse(IAsyncResult ar)
        {
            var helper = ar.AsyncState as HttpWebRequestHelper;
            if (helper == null)
                return;

            try
            {

                var response = helper.Request.EndGetResponse(ar) as HttpWebResponse;
                if (response == null)
                    return;

                var stream = response.GetResponseStream();
                if (stream == null)
                    return;

                using (var reader = new StreamReader(stream))
                {
                    helper.OnResponseComplete(new HttpResponseCompleteEventArgs(reader.ReadToEnd()));
                }
            }
            catch (Exception ex)
            {
                helper.OnResponseComplete(new HttpResponseCompleteEventArgs(ex));
                //TODO: Research why these errors potentially happen - and how to fix
                //Server.Trace(ex.ToString());
            }
        }
    }
}


