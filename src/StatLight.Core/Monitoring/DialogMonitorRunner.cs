namespace StatLight.Core.Monitoring
{
    using System;
    using System.Collections.Generic;
    using StatLight.Core.Common;
    using StatLight.Core.Events;
    using StatLight.Core.Events.Aggregation;
    using StatLight.Core.Timing;

    internal class DialogMonitorRunner
    {
        private readonly ILogger _logger;
        private readonly IEventAggregator _eventAggregator;
        private readonly ITimer _dialogPingingTimer;
        private readonly IList<IDialogMonitor> _dialogMonitors;
        private readonly Dictionary<Type, bool> _isMonitorCurrentlyRunning = new Dictionary<Type, bool>();

        public DialogMonitorRunner(ILogger logger, IEventAggregator eventAggregator, ITimer dialogPingingTimer, IList<IDialogMonitor> dialogMonitors)
        {
            if (dialogMonitors == null)
                throw new ArgumentNullException("dialogMonitors");

            _logger = logger;
            _eventAggregator = eventAggregator;
            _dialogPingingTimer = dialogPingingTimer;
            _dialogMonitors = dialogMonitors;

            foreach (var monitor in _dialogMonitors)
            {
                //_logger.Debug("adding dialogMonitor[{0}]".FormatWith(_isMonitorCurrentlyRunning.GetType()));
                _isMonitorCurrentlyRunning.Add(monitor.GetType(), false);
            }

            _dialogPingingTimer.Elapsed += DialogPingingTimerElapsed;
            _dialogPingingTimer.Enabled = true;
        }

        void DialogPingingTimerElapsed(object sender, TimerWrapperElapsedEventArgs e)
        {
            foreach (var dialogMonitor in _dialogMonitors)
            {
                //_logger.Debug("DialogMonitorRunner.Elapsed Start = {0}".FormatWith(dialogMonitor.ToString()));


                if (!_isMonitorCurrentlyRunning[dialogMonitor.GetType()])
                {
                    //_logger.Debug("DialogMonitorRunner.Elapsed - running = {0}".FormatWith(dialogMonitor.ToString()));

                    _isMonitorCurrentlyRunning[dialogMonitor.GetType()] = true;
                    ExecuteDialogSlapdown(dialogMonitor, dialogMonitor.DialogType);
                    _isMonitorCurrentlyRunning[dialogMonitor.GetType()] = false;
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
            Action<string> a = msg => _eventAggregator.SendMessage(
                new DialogAssertionServerEvent(dialogType)
                    {
                        Message = msg,
                    });

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