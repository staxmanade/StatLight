namespace StatLight.Core.WebBrowser
{
	using System;
	using System.Threading;
	using System.Windows.Forms;
	using StatLight.Core.Common;
	using StatLight.Core.Monitoring;
	using StatLight.Core.Timing;

	internal class BrowserFormHost : IBrowserFormHost, IDisposable
	{
		private readonly ILogger _logger;
		private readonly Uri _pageToHost;
		private readonly bool _browserVisible;
		private Thread _browserThread;
		private bool _shouldBrowserThreadQuit;
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

				var browser = new WebBrowser
				{
					Url = _pageToHost,
					Dock = DockStyle.Fill
				};

				_form.Controls.Add(browser);
				_form.Show();

				for (; ; )
				{
					Application.DoEvents();

					if (!_shouldBrowserThreadQuit)
						continue;

					_logger.Debug("_shouldBrowserThreadQuit switched to false");
					_shouldBrowserThreadQuit = false;
					break;
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
			_shouldBrowserThreadQuit = true;
		}

		public void Stop()
		{
			_logger.Debug("~BrowserFormHost.Stop()");
			_dialogMonitorRunner.Stop();
			_shouldBrowserThreadQuit = true;
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
