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

        public SelfHostedWebBrowser(ILogger logger, Uri pageToHost, bool browserVisible)
        {
            _logger = logger;
            _pageToHost = pageToHost;
            _browserVisible = browserVisible;
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

                var browser = new WebBrowser
                {
                    Url = _pageToHost,
                    Dock = DockStyle.Fill
                };

                _form.Controls.Add(browser);

                Application.Run(_form);
                
            });
            _browserThread.SetApartmentState(ApartmentState.STA);
            _browserThread.Start();

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
            _form.Invoke(new Action(() => _form.Close()));
            _browserThread = null;
        }

        public int? ProcessId
        {
            get { return Process.GetCurrentProcess().Id; }
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
