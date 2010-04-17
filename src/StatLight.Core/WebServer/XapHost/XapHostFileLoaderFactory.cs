using System.Collections.Generic;
using StatLight.Core.Common;
using System;
using StatLight.Core.UnitTestProviders;

namespace StatLight.Core.WebServer.XapHost
{
    public class XapHostFileLoaderFactory
    {
        private readonly ILogger _logger;
        private IDictionary<XapHostType, IXapHostFileLoader> XapHostFileLoaders { get; set; }

        public XapHostFileLoaderFactory(ILogger logger)
        {
            _logger = logger;
            XapHostFileLoaders = new Dictionary<XapHostType, IXapHostFileLoader>();

            // Define the MSTest versions
            foreach (object enumItem in Enum.GetValues(typeof(MicrosoftTestingFrameworkVersion)))
            {
                var enumItemCasted = (MicrosoftTestingFrameworkVersion)enumItem;
                var e = (XapHostType)Enum.Parse(typeof(XapHostType), "MSTest" + enumItemCasted);
                XapHostFileLoaders.Add(e, new DiskXapHostFileLoader(_logger, StatLightClientXapNames.ClientXapNameFormat.FormatWith(enumItemCasted)));
            }

            XapHostFileLoaders.Add(XapHostType.UnitDrivenDecember2009, new DiskXapHostFileLoader(_logger, StatLightClientXapNames.ClientXapNameFormat.FormatWith(XapHostType.UnitDrivenDecember2009)));
        }

        public virtual byte[] LoadXapHostFor(XapHostType version)
        {
            _logger.Debug("Loading XapHost file [" + version + "]");
            return XapHostFileLoaders[version].LoadXapHost();
        }

        public XapHostType MapToXapHostType(UnitTestProviderType unitTestProviderType, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion)
        {
            Action throwNotSupportedException = () =>
            {
                throw new NotSupportedException(
                    "Cannot map to a xap host based on the following input values UnitTestProviderType={0}, MicrosoftTestingFrameworkVersion={1}"
                        .FormatWith(unitTestProviderType, microsoftTestingFrameworkVersion));
            };

            switch (unitTestProviderType)
            {
                case UnitTestProviderType.NUnit:
                case UnitTestProviderType.XUnit:
                    return XapHostType.MSTestMarch2010;

                case UnitTestProviderType.MSTest:
                    switch (microsoftTestingFrameworkVersion)
                    {
                        case MicrosoftTestingFrameworkVersion.December2008: return XapHostType.MSTestDecember2008;
                        case MicrosoftTestingFrameworkVersion.March2009: return XapHostType.MSTestMarch2009;
                        case MicrosoftTestingFrameworkVersion.July2009: return XapHostType.MSTestJuly2009;
                        case MicrosoftTestingFrameworkVersion.October2009: return XapHostType.MSTestOctober2009;
                        case MicrosoftTestingFrameworkVersion.November2009: return XapHostType.MSTestNovember2009;
                        case MicrosoftTestingFrameworkVersion.March2010: return XapHostType.MSTestMarch2010;
                        default:
                            throwNotSupportedException();
                            break;
                    }
                    break;

                case UnitTestProviderType.UnitDriven:
                    return XapHostType.UnitDrivenDecember2009;

                case UnitTestProviderType.Undefined:
                default:
                    throwNotSupportedException();
                    break;
            }

            return XapHostType.UnitDrivenDecember2009;
        }
    }
}
