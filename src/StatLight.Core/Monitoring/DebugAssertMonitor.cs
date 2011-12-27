
namespace StatLight.Core.Monitoring
{
    using System;
    using System.Windows.Automation;
    using StatLight.Core.Common;
    using StatLight.Core.Events;

    internal class DebugAssertMonitor : IDialogMonitor
    {
        private readonly ILogger _logger;

        public DebugAssertMonitor(ILogger logger)
        {
            _logger = logger;
        }

        public DialogMonitorResult ExecuteDialogSlapDown(Action<string> ifSlappedAction)
        {
            if (ifSlappedAction == null) throw new ArgumentNullException("ifSlappedAction");
            var noActionTaken = DialogMonitorResult.NoSlapdownAction();

            var dialogWindow = TreeWalker.ControlViewWalker.GetFirstChild(AutomationElement.RootElement);
            while (dialogWindow != null)
            {
                if (dialogWindow.Current.Name.Contains("Assertion Failed"))
                {
                    _logger.Debug("Found an assertion dialog;");
                    break;
                }
                dialogWindow = TreeWalker.ControlViewWalker.GetFirstChild(dialogWindow);
            }

            if (dialogWindow == null)
            {
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

            var buttonClickInvokePattern = okButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;

            if (buttonClickInvokePattern == null)
            {
                _logger.Debug("COULD NOT FIND THE buttonClickInvokePattern");
                return noActionTaken;
            }

            string caption = dialogWindow.GetNamePropertyOf(AutomationTypes.ControlText);
            string text = dialogWindow.GetNamePropertyOf(AutomationTypes.ControlTitleBar);

            string msg = @"A Silverlight Debug Assertion dialog was automatically closed.
Caption: {0}
Dialog Message:
{1}".FormatWith(caption, text);

            ifSlappedAction(msg);

            // Close the dialgo by clicking OK
            buttonClickInvokePattern.Invoke();

            return new DialogMonitorResult
            {
                WasActionTaken = true,
                Message = msg,
            };

            throw new NotImplementedException();

            //var rootElement = AutomationElement.RootElement;

            //if (rootElement == null)
            //    return DialogMonitorResult.NoSlapdownAction();

            //var dialogWindow = TreeWalker.ControlViewWalker.GetFirstChild(rootElement);

            //while (dialogWindow != null)
            //{
            //    if (dialogWindow.Current.Name.Contains("Assertion Failed"))
            //    {
            //        _logger.Debug("DebugAssertMonitor. debug assertion dialog found");

            //        var dialogWindowChildNode = TreeWalker.ControlViewWalker.GetFirstChild(dialogWindow);
            //        while (dialogWindowChildNode != null)
            //        {
            //            if (dialogWindowChildNode.Current.ControlType.LocalizedControlType == "button"
            //               && dialogWindowChildNode.Current.Name.ToLower(CultureInfo.CurrentCulture) == "ok")
            //            {
            //                var buttonClickInvokePattern = dialogWindowChildNode.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;

            //                if (buttonClickInvokePattern != null)
            //                    buttonClickInvokePattern.Invoke();
            //                break;
            //            }

            //            dialogWindowChildNode = TreeWalker.ControlViewWalker.GetNextSibling(dialogWindowChildNode);
            //        }

            //        _logger.Debug("DebugAssertMonitor.RunTheDialogSlapdown");

            //        const string msg = "A Silverlight Debug.Assert() dialog was automatically closed.";

            //        ifSlappedAction(msg);

            //        return new DialogMonitorResult()
            //        {
            //            WasActionTaken = true,
            //            Message = msg,
            //        };
            //    }

            //    dialogWindow = TreeWalker.ControlViewWalker.GetNextSibling(dialogWindow);
            //}

            //return DialogMonitorResult.NoSlapdownAction();


        }

        public DialogType DialogType
        {
            get { return DialogType.Assert; }
        }


    }
}