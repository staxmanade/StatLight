using System;
using StatLight.Core.Properties;

namespace StatLight.Core.WebServer.Host
{
    public class ResponseFactory
    {
        private readonly Func<byte[]> _xapToTestFactory;
        private readonly byte[] _hostXap;
        private readonly string _serializedConfiguration;

        public ResponseFactory(Func<byte[]> xapToTestFactory, byte[] hostXap, string serializedConfiguration)
        {
            _xapToTestFactory = xapToTestFactory;
            _hostXap = hostXap;
            _serializedConfiguration = serializedConfiguration;
        }

        public string ClientAccessPolicy { get { return Resources.ClientAccessPolicy; } }
        public string CrossDomain { get { return Resources.CrossDomain; } }

        private static int _htmlPageInstanceId = 0;

        public ResponseFile Get(string localPath)
        {
            if (IsKnown(localPath, StatLightServiceRestApi.CrossDomain))
                return new ResponseFile { File = Resources.CrossDomain.ToByteArray(), ContentType = "text/xml" };

            if (IsKnown(localPath, StatLightServiceRestApi.ClientAccessPolicy))
                return new ResponseFile { File = Resources.ClientAccessPolicy.ToByteArray(), ContentType = "text/xml" };

            if (IsKnown(localPath, StatLightServiceRestApi.GetHtmlTestPage))
            {
                _htmlPageInstanceId++;
                return GetTestHtmlPage(_htmlPageInstanceId);
            }

            if (IsKnown(localPath, StatLightServiceRestApi.GetXapToTest))
                return new ResponseFile { File = _xapToTestFactory(), ContentType = "application/x-silverlight-app" };

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestPageHostXap))
                return new ResponseFile { File = _hostXap, ContentType = "application/x-silverlight-app" };

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestRunConfiguration))
                return new ResponseFile { File = _serializedConfiguration.ToByteArray(), ContentType = "text/xml" };

            throw new NotImplementedException();
        }

        public bool IsKnownFile(string localPath)
        {
            if (IsKnown(localPath, StatLightServiceRestApi.CrossDomain))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.ClientAccessPolicy))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetHtmlTestPage))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetXapToTest))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestPageHostXap))
                return true;

            if (IsKnown(localPath, StatLightServiceRestApi.GetTestRunConfiguration))
                return true;

            return false;
        }


        public static ResponseFile GetTestHtmlPage(int instanceId)
        {
            var page = Resources.TestPage.Replace("BB86D193-AD39-494A-AEB7-58F948BA5D93", instanceId.ToString());

            return new ResponseFile { File = page.ToByteArray(), ContentType = "text/html" };
        }

        private static bool IsKnown(string filea, string fileb)
        {
            return string.Equals(filea, fileb, StringComparison.OrdinalIgnoreCase);
        }
    }
}