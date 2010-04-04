using System.Collections.Generic;
using StatLight.Core.Common;
using System;

namespace StatLight.Core.WebServer.XapHost
{
    public class XapHostFileLoaderFactory
    {
        private readonly ILogger _logger;
        private IDictionary<MicrosoftTestingFrameworkVersion, IXapHostFileLoader> XapHostFileLoaders { get; set; }

        public XapHostFileLoaderFactory(ILogger logger)
        {
            _logger = logger;
            XapHostFileLoaders = new Dictionary<MicrosoftTestingFrameworkVersion, IXapHostFileLoader>();
            foreach (object enumItem in Enum.GetValues(typeof(MicrosoftTestingFrameworkVersion)))
            {
                var enumItemCasted = (MicrosoftTestingFrameworkVersion)enumItem;

                XapHostFileLoaders.Add(enumItemCasted, new DiskXapHostFileLoader(_logger, string.Format(StatLightClientXapNames.ClientXapNameFormat, enumItemCasted)));
            }
        }

        public virtual byte[] LoadXapHostFor(MicrosoftTestingFrameworkVersion version)
        {
            _logger.Debug("Loading XapHost file [" + version + "]");
            return XapHostFileLoaders[version].LoadXapHost();
        }
    }
}
