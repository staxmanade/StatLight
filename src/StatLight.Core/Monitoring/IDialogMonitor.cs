using System;
using StatLight.Core.Events;

namespace StatLight.Core.Monitoring
{
    internal interface IDialogMonitor
    {
        DialogMonitorResult ExecuteDialogSlapDown(Action<string> ifSlappedAction);
        DialogType DialogType { get; }
    }
}