using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    internal class ChromeWebBrowser : OutOfProcessWebBrowserBase
    {
        public ChromeWebBrowser(ILogger logger, Uri uri, bool forceBrowserStart, bool isStartingMultipleInstances)
            : base(logger, uri, forceBrowserStart, isStartingMultipleInstances)
        {
        }

        protected override string ProcessName
        {
            get { return "chrome"; }
        }

        protected override string ExePath
        {
            get { return ChromePath; }
        }

        public static string ChromePath
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


                return chrome;
            }
        }
    }
}