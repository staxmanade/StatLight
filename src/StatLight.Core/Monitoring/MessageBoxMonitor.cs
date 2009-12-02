
using System.Threading;

namespace StatLight.Core.Monitoring
{
	using System.Diagnostics;
	using System.Windows.Automation;
	using StatLight.Core.Common;
using System;

	internal class MessageBoxMonitor : IDialogMonitor
	{
		private readonly ILogger _logger;

		public MessageBoxMonitor(ILogger logger)
		{
			_logger = logger;
		}

		public DialogMonitorResult ExecuteDialogSlapDown()
		{
			var noActionTaken = DialogMonitorResult.NoSlapdownAction();

			//if(browserWindowHandle == IntPtr.Zero)
			//{
			//    _logger.Debug("browserWindowHandle is IntPtr.Zero");
			//    return noActionTaken;
			//}
			//AutomationElement appWindow = AutomationElement.FromHandle(browserWindowHandle);
			//if (appWindow == null)
			//{
			//    _logger.Debug("COULD NOT FIND THE AUTOMATION ELEMENT FOR browserWindowHandle {0}".FormatWith(browserWindowHandle));
			//    return noActionTaken;
			//}

			var processId = Process.GetCurrentProcess().Id;
			var processIdCond = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
			var appWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children, processIdCond);
			if (appWindow == null)
			{
				_logger.Debug("COULD NOT FIND THE AUTOMATION ELEMENT FOR processId {0}".FormatWith(processId));
				return noActionTaken;
			}

			var dialogCond = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "Dialog");
			var dialogWindow = appWindow.FindFirst(TreeScope.Children, dialogCond);
			if (dialogWindow == null)
			{
				_logger.Debug("COULD NOT FIND THE AUTOMATION ELEMENT FOR dialgWindow 'Dialog'");
				return noActionTaken;
			}

			var okButtonCond = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "button");
			var okButtonNameCond = new PropertyCondition(AutomationElement.NameProperty, "OK");
			var okCond = new AndCondition(okButtonCond, okButtonNameCond);
			var okButton = dialogWindow.FindFirst(TreeScope.Children, okCond);
			if (okButton == null)
			{
				_logger.Debug("COULD NOT FIND THE okButton");
				return noActionTaken;
			}

			var buttonClicInvokePattern = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;

			if (buttonClicInvokePattern == null)
			{
				_logger.Debug("COULD NOT FIND THE buttonClicInvokePattern");
				return noActionTaken;
			}

			// Close the dialgo by clicking OK
			buttonClicInvokePattern.Invoke();
			Thread.Sleep(100);
			return new DialogMonitorResult
					{
						WasActionTaken = true,
						Message = "A Silverlight MessageBox dialog was automatically closed.",
					};
		}
	}
}