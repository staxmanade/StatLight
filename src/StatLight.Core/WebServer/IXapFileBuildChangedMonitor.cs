
namespace StatLight.Core.WebServer
{
	using System;

	public interface IXapFileBuildChangedMonitor
	{
		event EventHandler<XapFileBuildChangedEventArgs> FileChanged;
	}
}
