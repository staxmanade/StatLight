using System;
using System.IO;
using System.Net;
using System.Windows.Browser;
using StatLight.Core.WebServer;

namespace StatLight.Client.Silverlight
{
	public static class StatLightPostbackManager
	{
		private static int postMessageCount = 0;
		public static void PostMessage(string message)
		{
			postMessageCount++;

			HttpPost(StatLightServiceRestApi.PostMessage.ToFullUri(), message);
		}

		/// <summary>
		/// Send a message back to the server signaling that all the tests have completed.
		/// </summary>
		public static void SignalTestComplete()
		{
			HttpGet(StatLightServiceRestApi.SignalTestComplete.Replace("{totalMessagesPostedCount}", postMessageCount.ToString()).ToFullUri());
		}

		private static void HttpPost(Uri uri, string message)
		{
			new HttpWebRequestHelper(uri, "POST", message).Execute();
		}

		private static void HttpGet(Uri uri)
		{
			WebClient client = new WebClient();
			client.DownloadStringCompleted += (sender, e) =>
			{
				if (e.Error != null)
					throw new NotImplementedException("Error Handling Not implemented...Current Error - " + e.Error.ToString());
			};
			client.DownloadStringAsync(uri);
		}
	}

	internal class HttpWebRequestHelper
	{
		private HttpWebRequest Request { get; set; }
		public string PostData { get; private set; }

		public event HttpResponseCompleteEventHandler ResponseComplete;

		private void OnResponseComplete(HttpResponseCompleteEventArgs e)
		{
			var handler = this.ResponseComplete;
			if (handler != null)
			{
				handler(e);
			}
		}

		public HttpWebRequestHelper(Uri requestUri, string method, string postData)
		{
			this.Request = (HttpWebRequest)WebRequest.Create(requestUri);
			this.Request.ContentType = "application/x-www-form-urlencoded";
			this.Request.Method = method;
			this.PostData = postData;
		}

		public void Execute()
		{
			this.Request.BeginGetRequestStream(BeginRequest, this);
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
						writer.Write(HttpUtility.UrlEncode(PostData));
						//foreach (var item in helper.PostData)
						//{
						//    writer.Write("{0}={1}&", item.Key, HttpUtility.UrlEncode(item.Value));
						//}
					}
				}
				helper.Request.BeginGetResponse(BeginResponse, helper);
			}
		}

		private static void BeginResponse(IAsyncResult ar)
		{
			try
			{
				var helper = ar.AsyncState as HttpWebRequestHelper;
				if (helper == null)
					return;

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
			catch (Exception)
			{
				//TODO: why in IE8 this bombs out, and yet, still seems to work fine?
			}
		}
	}
}
