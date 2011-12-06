using System;
using System.Net;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer
{
    public class WebServerLocation
    {
        private readonly ILogger _logger;
        private readonly int _defaultPortToTry;
        private readonly Lazy<int> _port;

        public WebServerLocation(ILogger logger)
            :this(logger, 8887)
        {
        }

        public WebServerLocation(ILogger logger, int defaultPortToTry)
        {
            _logger = logger;
            _defaultPortToTry = defaultPortToTry;
            _port = new Lazy<int>(GetUnusedPort);
        }

        public virtual Uri TestPageUrl
        {
            get
            {
                var uriString = GetBaseUrl(Port) + StatLightServiceRestApi.GetHtmlTestPage;
                return new Uri(uriString);
            }
        }

        public Uri BaseUrl
        {
            get
            {
                return GetBaseUrl(Port).ToUri();
            }
        }

        public  int Port
        {
            get { return _port.Value; }
        }

        private static string GetBaseUrl(int port)
        {
            return ("http://localhost:" + port + "/");
        }

        private int GetUnusedPort()
        {
            int port = _defaultPortToTry;

            while (!TryPortNumber(port))
                port++;

            return port;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool TryPortNumber(int port)
        {
            var url = GetBaseUrl(port);
            _logger.Debug("Attempting to open port at {0}".FormatWith(url));
            var server = new HttpListener();
            try
            {
                server.Prefixes.Add(url);
                server.Start();
                return true;
            }
            catch (Exception ex)
            {
                _logger.Debug(ex.ToString());
                return false;
            }
            finally
            {
                server.Close();
            }
        }
    }
}
