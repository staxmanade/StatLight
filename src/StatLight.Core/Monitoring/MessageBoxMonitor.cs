
using System.Threading;
using StatLight.Core.Events;

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

        public DialogMonitorResult ExecuteDialogSlapDown(Action<string> ifSlappedAction)
        {
            var noActionTaken = DialogMonitorResult.NoSlapdownAction();

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
                //_logger.Debug("COULD NOT FIND THE AUTOMATION ELEMENT FOR dialgWindow 'Dialog'");
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

            string caption = dialogWindow.GetNamePropertyOf(AutomationTypes.ControlText);
            string text = dialogWindow.GetNamePropertyOf(AutomationTypes.ControlTitleBar);

            string msg = @"A Silverlight MessageBox dialog was automatically closed.
Caption: {0}
Dialog Message:
{1}".FormatWith(caption, text);

            ifSlappedAction(msg);

            // Close the dialgo by clicking OK
            buttonClicInvokePattern.Invoke();

            return new DialogMonitorResult
                    {
                        WasActionTaken = true,
                        Message = msg,
                    };
        }

        public DialogType DialogType
        {
            get { return DialogType.MessageBox; }
        }
    }

    public static class AutomationTypes
    {
        public static string ControlTitleBar = "title bar";
        public static string ControlText = "text";

        public static string GetNamePropertyOf(this AutomationElement dialogWindow, string type)
        {
            var controlTypePropertyCondition = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, type);
            var firstResultOfCondition = dialogWindow.FindFirst(TreeScope.Children, controlTypePropertyCondition);
            return firstResultOfCondition.GetCurrentPropertyValue(AutomationElement.NameProperty) as string;
        }
    }

    public interface IAutomationWrapper
    {

    }
}