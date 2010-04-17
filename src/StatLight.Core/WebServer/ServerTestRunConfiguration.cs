using System;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer
{
    using StatLight.Core.WebServer.XapHost;

    public class ServerTestRunConfiguration
    {
        private readonly XapHostFileLoaderFactory _xapHostFileLoaderFactory;
        private readonly XapHostType _xapHostType;
        private byte[] _hostXap;
        public long DialogSmackDownElapseMilliseconds { get; set; }

        public ServerTestRunConfiguration(XapHostFileLoaderFactory xapHostFileLoaderFactory, XapHostType xapHostType)
        {
            _xapHostFileLoaderFactory = xapHostFileLoaderFactory;
            _xapHostType = xapHostType;

            DialogSmackDownElapseMilliseconds = 5000;
        }

        public byte[] HostXap
        {
            get
            {
                if (_hostXap == null)
                    _hostXap = _xapHostFileLoaderFactory.LoadXapHostFor(_xapHostType);

                return _hostXap;
            }
        }
    }
}
