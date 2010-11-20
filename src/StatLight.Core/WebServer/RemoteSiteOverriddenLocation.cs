using System;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer
{
    public class RemoteSiteOverriddenLocation : WebServerLocation
    {
        private readonly Uri _remotePath;

        public RemoteSiteOverriddenLocation(ILogger logger, Uri remotePath)
            : base(logger)
        {
            _remotePath = remotePath;
        }

        public override Uri TestPageUrl
        {
            get { return _remotePath; }
        }
    }
}