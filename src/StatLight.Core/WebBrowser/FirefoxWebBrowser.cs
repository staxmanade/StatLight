using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    internal class FirefoxWebBrowser : OutOfProcessWebBrowserBase
    {
        private readonly bool _forceBrowserStart;
        private static bool _isStartingMultipleInstances;
        public FirefoxWebBrowser(ILogger logger, Uri uri, bool forceBrowserStart, bool isStartingMultipleInstances)
            : base(logger, uri)
        {
            _forceBrowserStart = forceBrowserStart;
            _isStartingMultipleInstances = isStartingMultipleInstances;
        }

        protected override string ExePath
        {
            get { return FireFoxPath; }
        }

        protected override string GetCommandLineArguments(Uri uri)
        {
            return "{0} {1}".FormatWith("-new-window", base.GetCommandLineArguments(uri));
        }

        public override void Start()
        {
            if (AFirefoxProcessAlreadyExists())
            {
                if (!_isStartingMultipleInstances)
                {
                    if (_forceBrowserStart)
                    {
                        KillFirefox(this);
                    }
                    else
                        throw new StatLightException("An instance of the Firefox process is currently open. You can choose the --ForceBrowserStart option to kill existing instances before StatLight runs.");
                }
            }

            base.Start();
        }

        protected override void Close()
        {
            KillFirefox(this);
        }

        public static void KillFirefox()
        {
            KillFirefox(null);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public static void KillFirefox(FirefoxWebBrowser firefoxWebBrowser)
        {
            foreach (var process in GetFirefoxProcesses())
            {
                Console.WriteLine(process.ProcessName);
                try
                {
                    process.Kill();
                }
                catch (Exception ex)
                {
                    firefoxWebBrowser._logger.Debug(ex.ToString());
                }
            }
            if (firefoxWebBrowser != null)
                firefoxWebBrowser.Process = null;
            System.Threading.Thread.Sleep(500);
        }

        private static bool AFirefoxProcessAlreadyExists()
        {
            return GetFirefoxProcesses().Any();
        }

        internal static IEnumerable<Process> GetFirefoxProcesses()
        {
            return Process.GetProcessesByName("firefox");
        }

        public static bool IsFirefoxInstalled()
        {
            if (File.Exists(FireFoxPath))
                return true;

            return false;
        }

        public static string FireFoxPath
        {
            get
            {
                // These build tasks should be fun in 32-bit.
                string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

                if (!Directory.Exists(pf))
                {
                    throw new InvalidOperationException("Could not locate the application directory.");
                }

                string firefox =
                    Path.Combine(
                    Path.Combine(
                    pf,
                    "Mozilla Firefox"),
                    "firefox.exe");

                return firefox;
            }
        }
    }
}