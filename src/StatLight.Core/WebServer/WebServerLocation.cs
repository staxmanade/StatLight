using System;
using System.Net;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer
{
    public class WebServerLocation
    {
        private readonly ILogger _logger;

        public WebServerLocation(ILogger logger)
        {
            _logger = logger;
        }

        public virtual Uri TestPageUrl
        {
            get
            {
                var uriString = GetBaseUrl() + StatLightServiceRestApi.GetHtmlTestPage;
                return new Uri(uriString);
            }
        }

        public Uri BaseUrl
        {
            get
            {
                return GetBaseUrl().ToUri();
            }
        }

        private string GetBaseUrl()
        {
            return ("http://localhost:" + GetUnusedPort() + "/");
        }

        private int GetUnusedPort()
        {
            int port = 8887;

            while (!TryPortNumber(port))
                port++;

            return port;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private bool TryPortNumber(int port)
        {
            var url = "http://localhost:{0}/".FormatWith(port);

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
