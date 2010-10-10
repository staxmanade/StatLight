
using System;
namespace StatLight.Core.WebBrowser
{
	public interface IWebBrowser : IDisposable
	{
		void Start();
		void Stop();
	}
}
