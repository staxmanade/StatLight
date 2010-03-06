
using System;
namespace StatLight.Core.WebBrowser
{
	public interface IBrowserFormHost : IDisposable
	{
		void Start();
		void Stop();
	}
}
