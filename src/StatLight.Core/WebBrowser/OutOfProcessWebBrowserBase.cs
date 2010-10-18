using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    /// <summary>
    /// A simple web browser wrapper that allows any simple process to be used
    /// as a web browser instance if it takes a URL in the startup parameters.
    /// </summary>
    internal abstract class OutOfProcessWebBrowserBase : IWebBrowser
    {
        /// <summary>
        /// Initializes a new instance of the OutOfProcessWebBrowser type with a
        /// provided path to the web browser.
        /// </summary>
        protected OutOfProcessWebBrowserBase(ILogger logger, Uri uri)
        {
            _logger = logger;
            _uri = uri;
        }

        protected abstract string ExePath { get; }

        /// <summary>
        /// Gets or sets the location of the web browser process.
        /// </summary>
        private string Executable
        {
            get
            {
                if (!File.Exists(ExePath))
                {
                    throw new FileNotFoundException("The web browser could not be located on the system.", ExePath);
                }

                return ExePath;
            }
        }

        /// <summary>
        /// Gets or sets the process instance.
        /// </summary>
        protected Process Process { get; set; }

        protected readonly ILogger _logger;
        private readonly Uri _uri;

        /// <summary>
        /// Gets a value indicating whether the browser process is currently
        /// running.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to keep requests going.")]
        public bool IsRunning
        {
            get
            {
                try
                {
                    return ((Process != null) && !Process.HasExited);
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets or sets the process id.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to keep requests going.")]
        public int? ProcessId
        {
            get
            {
                if (Process == null)
                    return null;

                return Process.Id;
            }
        }

        /// <summary>
        /// Retrieves the command line arguments to use when starting the
        /// executable.
        /// </summary>
        /// <param name="uri">The web address to browse to.</param>
        /// <returns>Returns the command line arguments.</returns>
        protected virtual string GetCommandLineArguments(Uri uri)
        {
            return uri.ToString();
        }

        /// <summary>
        /// Starts the web browser pointing to a particular address.
        /// </summary>
        public virtual void Start()
        {
            // Start the browser process
            if (Process == null && !string.IsNullOrEmpty(Executable))
            {
                var psi = new ProcessStartInfo(
                    Executable,
                    GetCommandLineArguments(_uri));
                Process = Process.Start(psi);
            }
        }

        /// <summary>
        /// Closes the process.
        /// </summary>
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "Need to keep requests going.")]
        protected virtual void Close()
        {
            try
            {
                TimeSpan pollDelta = TimeSpan.FromMilliseconds(100);
                if (Process != null)
                {
                    // Not the cleanest implementation ever, will look to 
                    // improve as time allows. Inconsistent performance on
                    // terminal services.
                    if (Process.MainWindowHandle != IntPtr.Zero)
                    {
                        // 3 seconds worth of attempts
                        for (int i = 0; i < 30; i++)
                        {
                            if (Process.HasExited)
                            {
                                break;
                            }

                            Process.CloseMainWindow();

                            Thread.Sleep(pollDelta);
                        }
                    }
                }

                if (Process != null && !Process.HasExited)
                {
                    Process.Close();
                    Thread.Sleep(TimeSpan.FromSeconds(2));
                }
                if (Process != null && !Process.HasExited)
                {
                    Process.Kill();
                }
            }
            catch (Win32Exception ex)
            {
                _logger.Debug("Browser shutdown exception: " + ex);
            }
            catch (InvalidOperationException ex)
            {
                _logger.Debug("Browser shutdown exception: " + ex);
            }
            catch (Exception ex)
            {
                _logger.Debug("Browser shutdown exception: " + ex);
            }
            finally
            {
                _logger.Debug("Browser closed.");
            }
        }

        void IDisposable.Dispose()
        {
            Stop();
        }

        public void Stop()
        {
            Close();
        }
    }
}