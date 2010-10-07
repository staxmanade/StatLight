using System;

namespace StatLight.Core.WebServer
{
    public class WebServerLocation
    {
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

        private static string GetBaseUrl()
        {
            return ("http://localhost:" + GetUnusedPort() + "/");
        }

        private static int GetUnusedPort()
        {
            int port = 8887;

            while (!TryPortNumber(port))
                port++;

            return port;
        }

        private static bool TryPortNumber(int port)
        {
            try
            {
                using (var client = new System.Net.Sockets.TcpClient(new System.Net.IPEndPoint(System.Net.IPAddress.Any, port)))
                {
                    return true;
                }
            }
            catch (System.Net.Sockets.SocketException error)
            {
                if (error.SocketErrorCode == System.Net.Sockets.SocketError.AddressAlreadyInUse ||
                    error.SocketErrorCode == System.Net.Sockets.SocketError.AccessDenied)
                    return false;

                /* unexpected error that we DON'T have handling for here */
                throw;
            }
        }
    }
}
