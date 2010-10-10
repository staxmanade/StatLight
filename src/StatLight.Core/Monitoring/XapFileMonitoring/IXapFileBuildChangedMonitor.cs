
using System;

namespace StatLight.Core.Monitoring.XapFileMonitoring
{
    public interface IXapFileBuildChangedMonitor
	{
		event EventHandler<XapFileBuildChangedEventArgs> FileChanged;
	}
}
