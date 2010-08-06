using System;

namespace StatLight.Core.WebServer
{
    public class RemoteSiteOverriddenLocation : WebServerLocation
    {
        private readonly Uri _remotePath;

        public RemoteSiteOverriddenLocation(Uri remotePath)
        {
            _remotePath = remotePath;
        }

        public override Uri TestPageUrl
        {
            get { return _remotePath; }
        }
    }
}