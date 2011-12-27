using System;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using System.Windows.Automation;
using StatLight.Core.Common;
using StatLight.Core.Events;
using StatLight.Core.Monitoring;

namespace StatLight.WindowsPhoneEmulator
{
    public class OneTimePhoneEmulatorDialogMonitor
    {
        private readonly ILogger _logger;
        private Timer _timer;
        private WindowsPhoneEmulatorDialogMonitor _windowsPhoneEmulatorDialogMonitor;

        public OneTimePhoneEmulatorDialogMonitor(ILogger logger)
        {
            _logger = logger;
            _windowsPhoneEmulatorDialogMonitor = new WindowsPhoneEmulatorDialogMonitor(logger);
            _timer = new Timer(1000);
            _timer.Elapsed += (sender, e) => _windowsPhoneEmulatorDialogMonitor.ExecuteDialogSlapDown(msg =>
            {
                _logger.Information(msg);
                _timer.Stop();
                _timer = null;
                _windowsPhoneEmulatorDialogMonitor = null;
            });
            _timer.Start();
        }
    }

    public class WindowsPhoneEmulatorDialogMonitor
    {
        private readonly ILogger _logger;

        public WindowsPhoneEmulatorDialogMonitor(ILogger logger)
        {
            _logger = logger;
        }

        public DialogMonitorResult ExecuteDialogSlapDown(Action<string> ifSlappedAction)
        {
            var noActionTaken = DialogMonitorResult.NoSlapdownAction();

            AutomationElement appWindow;
            Process[] processesByName = Process.GetProcessesByName("XDE");

            if (processesByName.Any())
            {
                int processId = processesByName.First().Id;
                var processIdCond = new PropertyCondition(AutomationElement.ProcessIdProperty, processId);
                appWindow = AutomationElement.RootElement.FindFirst(TreeScope.Children, processIdCond);

                if (appWindow == null)
                {
                    _logger.Debug("COULD NOT FIND THE AUTOMATION ELEMENT FOR processId {0}".FormatWith(processId));
                    return noActionTaken;
                }
            }
            else
            {
                return noActionTaken;
            }

            if (appWindow.Current.Name.Contains("Assertion Failed"))
                return noActionTaken;

            var yesButtonCond = new PropertyCondition(AutomationElement.LocalizedControlTypeProperty, "button");
            var yesButtonNameCond = new PropertyCondition(AutomationElement.NameProperty, "Yes");
            var yesCond = new AndCondition(yesButtonCond, yesButtonNameCond);
            var yesButton = appWindow.FindFirst(TreeScope.Children, yesCond);
            if (yesButton == null)
            {
                _logger.Debug("COULD NOT FIND THE yesButton");
                return noActionTaken;
            }

            var buttonClicInvokePattern = yesButton.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;

            if (buttonClicInvokePattern == null)
            {
                _logger.Debug("COULD NOT FIND THE buttonClicInvokePattern");
                return noActionTaken;
            }

            string caption = appWindow.GetNamePropertyOf(AutomationTypes.ControlText);
            string text = appWindow.GetNamePropertyOf(AutomationTypes.ControlTitleBar);

            string msg = @"Windows Phone Emulator dialog was automatically closed.
Caption: {0}
Message:
{1}".FormatWith(caption, text);

            ifSlappedAction(msg);

            _logger.Debug("Clicking Yes on a dialog (MessageBox)");

            // Close the dialgo by clicking Yes
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
}