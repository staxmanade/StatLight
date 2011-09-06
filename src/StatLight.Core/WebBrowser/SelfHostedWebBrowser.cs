using System.Diagnostics;

namespace StatLight.Core.WebBrowser
{
    using System;
    using System.Threading;
    using System.Windows.Forms;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;

    internal class SelfHostedWebBrowser : IWebBrowser, IDisposable
    {
        private readonly ILogger _logger;
        private readonly Uri _pageToHost;
        private readonly bool _browserVisible;
        private Thread _browserThread;
        private readonly WindowGeometry _windowGeometry;

        public SelfHostedWebBrowser(ILogger logger, Uri pageToHost, bool browserVisible, WindowGeometry windowGeometry)
        {
            _logger = logger;
            _pageToHost = pageToHost;
            _browserVisible = browserVisible;
            if (windowGeometry == null)
            {
                _windowGeometry = new WindowGeometry() { WindowSize = new StatLight.Core.Configuration.Size(800, 600), WindowState = BrowserWindowState.Normal };
            }
            else
            {
                _windowGeometry = windowGeometry;
            }
        }

        private Form _form;
        public void Start()
        {
            _browserThread = new Thread(() =>
            {
                _form = new Form
                            {
                                Height = _windowGeometry.WindowSize.Height,
                                Width = _windowGeometry.WindowSize.Width,
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

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:LiteralsShouldBeSpelledCorrectly", MessageId="BrowserWindowState")]
        private FormWindowState GetBrowserVisibilityState(bool browserVisible)
        {
            if(browserVisible)
            {
                switch (_windowGeometry.WindowState)
                {
                    case BrowserWindowState.Maximized:
                        return FormWindowState.Maximized;
                    case BrowserWindowState.Minimized:
                        return FormWindowState.Minimized;
                    case BrowserWindowState.Normal:
                        return FormWindowState.Normal;
                    default:
                        throw new NotSupportedException("Cannot handle unknown BrowserWindowState");
                }
            }
            else
            {
                return FormWindowState.Minimized;
            }
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
