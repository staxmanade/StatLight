using System;

namespace StatLight.Client.Silverlight
{
	internal sealed class HttpResponseCompleteEventArgs : EventArgs
	{
		public string Response { get; set; }

		public HttpResponseCompleteEventArgs(string response)
		{
			this.Response = response;
		}
	}
}