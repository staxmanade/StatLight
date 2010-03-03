using System;

namespace StatLight.Core.Monitoring
{
	internal interface IDialogMonitor
	{
		DialogMonitorResult ExecuteDialogSlapDown(Action<string> ifSlappedAction);
	}
}