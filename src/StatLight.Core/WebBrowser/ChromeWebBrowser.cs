using System;
using System.IO;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    internal class ChromeWebBrowser : OutOfProcessWebBrowserBase
    {
        public ChromeWebBrowser(ILogger logger, Uri uri)
            : base(logger, uri)
        {
        }

        protected override string ExePath
        {
            get
            {
                string lad = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (string.IsNullOrEmpty(lad))
                {
                    lad = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                }

                if (!Directory.Exists(lad))
                {
                    throw new InvalidOperationException("Could not locate the local application data directory.");
                }

                string chrome = Path.Combine(
                    Path.Combine(
                    Path.Combine(
                    Path.Combine(
                    lad,
                    "Google"),
                    "Chrome"),
                    "Application"),
                    "chrome.exe");

                if (!File.Exists(chrome))
                {
                    throw new FileNotFoundException("The Chrome web browser application could not be located on this system.", chrome);
                }
                return chrome;
            }
        }
    }
}