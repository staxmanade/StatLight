namespace StatLight.Core.WebBrowser
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using StatLight.Core.Common;
    using StatLight.Core.Monitoring;

    internal class BrowserFormHost : IBrowserFormHost, IDisposable
    {
        private readonly ILogger _logger;
        private readonly Uri _pageToHost;
        private readonly bool _browserVisible;
        private Thread _browserThread;
        private readonly DialogMonitorRunner _dialogMonitorRunner;

        public BrowserFormHost(ILogger logger, Uri pageToHost, bool browserVisible, DialogMonitorRunner dialogMonitorRunner)
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

                using (_form)
                {
                    var browser = new WebBrowser
                    {
                        Url = _pageToHost,
                        Dock = DockStyle.Fill
                    };

                    _form.Controls.Add(browser);
                    _form.ShowDialog();
                }
            });
            _browserThread.SetApartmentState(ApartmentState.STA);
            _browserThread.Start();

            _dialogMonitorRunner.Start();
        }

        private static FormWindowState GetBrowserVisibilityState(bool browserVisible)
        {
            return browserVisible ? FormWindowState.Normal : FormWindowState.Minimized;
        }

        ~BrowserFormHost()
        {
            _logger.Debug("Disposing BrowserFormHost");
        }

        public void Stop()
        {
            _logger.Debug("BrowserFormHost.Stop()");
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
