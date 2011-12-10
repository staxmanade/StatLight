namespace StatLight.Core.Monitoring
{
    using System;
    using System.Collections.Generic;
    using StatLight.Core.Common;
    using StatLight.Core.Common.Abstractions.Timing;
    using StatLight.Core.Events;
    using StatLight.Core.Properties;

    internal class DialogMonitorRunner : IDialogMonitorRunner
    {
        private readonly ILogger _logger;
        private readonly IEventPublisher _eventPublisher;
        private readonly ITimer _dialogPingingTimer;
        private readonly IList<IDialogMonitor> _dialogMonitors;
        private readonly Dictionary<int, bool> _isMonitorCurrentlyRunning = new Dictionary<int, bool>();
        
        public DialogMonitorRunner(ILogger logger, IEventPublisher eventPublisher, IList<IDialogMonitor> dialogMonitors)
            : this(logger, eventPublisher, dialogMonitors, new TimerWrapper(Settings.Default.DialogSmackDownElapseMilliseconds))
        {}

        public DialogMonitorRunner(ILogger logger, IEventPublisher eventPublisher, IList<IDialogMonitor> dialogMonitors, ITimer dialogPingingTimer)
        {
            if (dialogMonitors == null)
                throw new ArgumentNullException("dialogMonitors");

            _logger = logger;
            _eventPublisher = eventPublisher;
            _dialogPingingTimer = dialogPingingTimer;
            _dialogMonitors = dialogMonitors;

            foreach (var monitor in _dialogMonitors)
            {
                //_logger.Debug("adding dialogMonitor[{0}]".FormatWith(_isMonitorCurrentlyRunning.GetType()));
                _isMonitorCurrentlyRunning.Add(monitor.GetHashCode(), false);
            }

            _dialogPingingTimer.Elapsed += DialogPingingTimerElapsed;
            _dialogPingingTimer.Enabled = true;
        }

        void DialogPingingTimerElapsed(object sender, TimerWrapperElapsedEventArgs e)
        {
            foreach (var dialogMonitor in _dialogMonitors)
            {
                //_logger.Debug("DialogMonitorRunner.Elapsed Start = {0}".FormatWith(dialogMonitor.ToString()));

                int dialogId = dialogMonitor.GetHashCode();

                if (!_isMonitorCurrentlyRunning[dialogId])
                {
                    //_logger.Debug("DialogMonitorRunner.Elapsed - running = {0}".FormatWith(dialogMonitor.ToString()));

                    _isMonitorCurrentlyRunning[dialogId] = true;
                    ExecuteDialogSlapdown(dialogMonitor, dialogMonitor.DialogType);
                    _isMonitorCurrentlyRunning[dialogId] = false;
                }
                else
                {
                    //_logger.Debug("DialogMonitorRunner.Elapsed skipped = {0}".FormatWith(dialogMonitor.ToString()));
                }
                //_logger.Debug("DialogMonitorRunner.Elapsed End = {0}".FormatWith(dialogMonitor.ToString()));
            }
        }

        private void ExecuteDialogSlapdown(IDialogMonitor dialogMonitor, DialogType dialogType)
        {
            Action<string> a = msg =>
                                   {
                                       if (msg.Contains("836D4425-DB59-48BB-BA7B-03AB20A57499"))
                                       {
                                           _eventPublisher
                                               .SendMessage(new FatalSilverlightExceptionServerEvent(dialogType) { Message = msg, });

                                           _eventPublisher
                                               .SendMessage<TestRunCompletedServerEvent>();

                                       }
                                       else
                                       {
                                           _eventPublisher.SendMessage(
                                               new DialogAssertionServerEvent(dialogType)
                                                   {
                                                       Message = msg,
                                                   });
                                       }
                                   };

            dialogMonitor.ExecuteDialogSlapDown(a);
        }

        public void Start()
        {
            _dialogPingingTimer.Start();
        }
        public void Stop()
        {
            _dialogPingingTimer.Stop();
        }
    }
}