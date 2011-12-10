using System;
using System.Threading;
using StatLight.Core.Properties;
using StatLight.Core.Serialization;

namespace StatLight.Core.WebServer
{
    public class ResponseFactory
    {
        private readonly ICurrentStatLightConfiguration _currentStatLightConfiguration;
        private int _htmlPageInstanceId;

        public ResponseFactory(ICurrentStatLightConfiguration currentStatLightConfiguration)
        {
            _currentStatLightConfiguration = currentStatLightConfiguration;
        }

        public ResponseFile Get(string localPath)
        {
            if (IsKnown(localPath, StatLightServiceRestApi.CrossDomain))
                return new ResponseFile { FileData = Resources.CrossDomain.ToByteArray(), ContentType = "text/xml" };

            if (IsKnown(localPath, StatLightServiceRestApi.ClientAccessPolicy))
                return new ResponseFile { FileData = Resources.ClientAccessPolicy.ToByteArray(), ContentType = "text/xml" };

            if (IsKnown(localPath, StatLightServiceRestApi.GetHtmlTestPage))
            {
                return GetTestHtmlPage();
            }

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestPageHostXap))
                return new ResponseFile { FileData = _currentStatLightConfiguration.Current.Server.HostXap(), ContentType = "application/x-silverlight-app" };

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestRunConfiguration))
                return new ResponseFile { FileData = _currentStatLightConfiguration.Current.Client.Serialize().ToByteArray(), ContentType = "text/xml" };

            throw new NotImplementedException();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public bool IsKnownFile(string localPath)
        {
            if (IsKnown(localPath, StatLightServiceRestApi.CrossDomain))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.ClientAccessPolicy))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetHtmlTestPage))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestPageHostXap))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestRunConfiguration))
                return true;

            return false;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        public ResponseFile GetTestHtmlPage()
        {
            string page = new TestPage(
                    instanceId: _htmlPageInstanceId, 
                    windowless: Settings.Default.Windowless
                ).ToString();

            Interlocked.Increment(ref _htmlPageInstanceId);

            return new ResponseFile { FileData = page.ToByteArray(), ContentType = "text/html" };
        }

        private static bool IsKnown(string filea, string fileb)
        {
            return string.Equals(filea, fileb, StringComparison.OrdinalIgnoreCase);
        }

        public void Reset()
        {
            _htmlPageInstanceId = 0;
        }
    }
}