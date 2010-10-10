using System.Diagnostics;

namespace StatLight.Core.WebBrowser
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using StatLight.Core.Common;
    using StatLight.Core.Monitoring;

    internal class SelfHostedWebBrowser : IWebBrowser, IDisposable
    {
        private readonly ILogger _logger;
        private readonly Uri _pageToHost;
        private readonly bool _browserVisible;
        private Thread _browserThread;
        private readonly IDialogMonitorRunner _dialogMonitorRunner;

        public SelfHostedWebBrowser(ILogger logger, Uri pageToHost, bool browserVisible, IDialogMonitorRunner dialogMonitorRunner)
        {
            _logger = logger;
            _pageToHost = pageToHost;
            _browserVisible = browserVisible;
            _dialogMonitorRunner = dialogMonitorRunner;
        }

        private Form _form;
        public void Start()
        {
            _browserThread = new Thread(() =>
            {
                _form = new Form
                            {
                                Height = 600,
                                Width = 800,
                                WindowState = GetBrowserVisibilityState(_browserVisible),
                                ShowInTaskbar = _browserVisible,
                                Icon = Properties.Resources.FavIcon,
                                Text = "StatLight - Browser Host"
                            };

                var browser = new System.Windows.Forms.WebBrowser
                {
                    Url = _pageToHost,
                    Dock = DockStyle.Fill
                };

                _form.Controls.Add(browser);

                Application.Run(_form);
                
            });
            _browserThread.SetApartmentState(ApartmentState.STA);
            _browserThread.Start();

            _dialogMonitorRunner.Start();
        }

        private static FormWindowState GetBrowserVisibilityState(bool browserVisible)
        {
            return browserVisible ? FormWindowState.Normal : FormWindowState.Minimized;
        }

        ~SelfHostedWebBrowser()
        {
            _logger.Debug("Disposing webBrowser");
        }

        public void Stop()
        {
            _logger.Debug("webBrowser.Stop()");
            _dialogMonitorRunner.Stop();
            _form.Close();
            _browserThread = null;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
                return;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

    }
}
