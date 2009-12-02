using System;

namespace StatLight.Core.Monitoring
{
	using System.Globalization;
	using System.Windows.Automation;
	using StatLight.Core.Common;

	internal class DebugAssertMonitor : IDialogMonitor
	{
		private readonly ILogger _logger;

		public DebugAssertMonitor(ILogger logger)
		{
			_logger = logger;
		}

		public DialogMonitorResult ExecuteDialogSlapDown()
		{
			//TODO: 
			if (!FoundSomeDialogWindowAndGaveItASmackDown())
				return DialogMonitorResult.NoSlapdownAction();

			_logger.Debug("DebugAssertMonitor.RunTheDialogSlapdown");

			return new DialogMonitorResult()
				{
					WasActionTaken = true,
					Message = "A Silverlight Debug.Assert() dialog was automatically closed.",
				};
		}

		private bool FoundSomeDialogWindowAndGaveItASmackDown()
		{
			var rootElement = AutomationElement.RootElement;

			if (rootElement == null)
				return false;

			var elementNode = TreeWalker.ControlViewWalker.GetFirstChild(rootElement);

			return ClosedWindowFromChildElement(elementNode);
		}

		private bool ClosedWindowFromChildElement(AutomationElement elementNode)
		{
			while (elementNode != null)
			{
				if (ClosedWindow(elementNode))
					return true;

				elementNode = TreeWalker.ControlViewWalker.GetNextSibling(elementNode);
			}

			return false;
		}

		private bool ClosedWindow(AutomationElement elementNode)
		{
			if (FoundWindowToClose(elementNode))
			{
				_logger.Debug("DebugAssertMonitor. debug assertion dialog found");
				var childNode = TreeWalker.ControlViewWalker.GetFirstChild(elementNode);
				DealWithAssertionFailedWindow(childNode);
				return true;
			}

			return false;
		}

		private static bool FoundWindowToClose(AutomationElement elementNode)
		{
			return elementNode.Current.Name.Contains("Assertion Failed");
		}

		private static void DealWithAssertionFailedWindow(AutomationElement elementNode)
		{
			while (elementNode != null)
			{
				if (IfItsTheOkButton(elementNode))
				{
					InvokeClickOnButton(elementNode);
					break;
				}

				elementNode = TreeWalker.ControlViewWalker.GetNextSibling(elementNode);
			}
		}

		private static void InvokeClickOnButton(AutomationElement elementNode)
		{
			var buttonClicInvokePattern = elementNode.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;

			if (buttonClicInvokePattern != null)
				buttonClicInvokePattern.Invoke();
		}

		private static bool IfItsTheOkButton(AutomationElement elementNode)
		{
			return elementNode.Current.ControlType.LocalizedControlType == "button"
				   && elementNode.Current.Name.ToLower(CultureInfo.CurrentCulture) == "ok";
		}
	}
}