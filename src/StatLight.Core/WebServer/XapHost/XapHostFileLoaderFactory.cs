using System.Collections.Generic;
using System.IO;
using System.Reflection;
using StatLight.Core.Common;
using System;
using StatLight.Core.Configuration;
using StatLight.Core.WebBrowser;

namespace StatLight.Core.WebServer.XapHost
{
    public class XapHostFileLoaderFactory
    {
        public static readonly string ClientXapNameFormat = "StatLight.Client.For.{0}.xap";

        private readonly ILogger _logger;
        private IDictionary<XapHostType, IXapHostFileLoader> XapHostFileLoaders { get; set; }


        public XapHostFileLoaderFactory(ILogger logger)
            : this(logger, Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
        {
        }

        public XapHostFileLoaderFactory(ILogger logger, string xapDirectory)
        {
            _logger = logger;
            XapHostFileLoaders = new Dictionary<XapHostType, IXapHostFileLoader>();

            // Define the MSTest versions
            foreach (object enumItem in Enum.GetValues(typeof(MicrosoftTestingFrameworkVersion)))
            {
                var enumItemCasted = (MicrosoftTestingFrameworkVersion)enumItem;
                var e = (XapHostType)Enum.Parse(typeof(XapHostType), "MSTest" + enumItemCasted);
                XapHostFileLoaders.Add(e, new DiskXapHostFileLoader(_logger, xapDirectory, ClientXapNameFormat.FormatWith(enumItemCasted)));
            }

            XapHostFileLoaders.Add(XapHostType.MSTestMay2010Phone, new DiskXapHostFileLoader(_logger, xapDirectory, ClientXapNameFormat.FormatWith(XapHostType.MSTestMay2010Phone)));
            XapHostFileLoaders.Add(XapHostType.UnitDrivenDecember2009, new DiskXapHostFileLoader(_logger, xapDirectory, ClientXapNameFormat.FormatWith(XapHostType.UnitDrivenDecember2009)));
            XapHostFileLoaders.Add(XapHostType.XunitContribApril2011, new DiskXapHostFileLoader(_logger, xapDirectory, ClientXapNameFormat.FormatWith(XapHostType.XunitContribApril2011)));
        }

        public virtual byte[] LoadXapHostFor(XapHostType version)
        {
            _logger.Debug("Loading XapHost file [" + version + "]");
            return XapHostFileLoaders[version].LoadXapHost();
        }

        public XapHostType MapToXapHostType(UnitTestProviderType unitTestProviderType, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, bool isPhoneRun)
        {
            Action throwNotSupportedException = () =>
            {
                throw new NotSupportedException(
                    "Cannot map to a xap host based on the following input values UnitTestProviderType={0}, MicrosoftTestingFrameworkVersion={1}"
                        .FormatWith(unitTestProviderType, microsoftTestingFrameworkVersion));
            };

            if (isPhoneRun)
            {
                switch (unitTestProviderType)
                {
                    case UnitTestProviderType.NUnit:
                    case UnitTestProviderType.XUnitLight:
                    case UnitTestProviderType.MSTest:
                        if(microsoftTestingFrameworkVersion != null && microsoftTestingFrameworkVersion == MicrosoftTestingFrameworkVersion.May2010)
                        return XapHostType.MSTestMay2010Phone;

                        throwNotSupportedException();
                        break;

                    case UnitTestProviderType.MSTestWithCustomProvider:
                        throw new NotSupportedException("Could possibly be supported - but just not done yet");

                    case UnitTestProviderType.UnitDriven:
                        throw new NotSupportedException("Could possibly be supported - but just not done yet");

                    case UnitTestProviderType.Undefined:
                    default:
                        throwNotSupportedException();
                        break;
                }
            }
            else
            {
                switch (unitTestProviderType)
                {
                    case UnitTestProviderType.NUnit:
                    case UnitTestProviderType.XUnit:
                        return XapHostType.MSTestMay2010;

                    case UnitTestProviderType.MSTestWithCustomProvider:
                    case UnitTestProviderType.MSTest:

                        if (microsoftTestingFrameworkVersion.HasValue)
                        {
                            var msTestVersionXapHostStringName = "MSTest" + microsoftTestingFrameworkVersion.Value;

                            if (Enum.IsDefined(typeof(XapHostType), msTestVersionXapHostStringName))
                                return (XapHostType)Enum.Parse(typeof(XapHostType), msTestVersionXapHostStringName);
                        }

                        throwNotSupportedException();
                        break;

                case UnitTestProviderType.UnitDriven:
                    return XapHostType.UnitDrivenDecember2009;
                case UnitTestProviderType.UnitDriven:
                    return XapHostType.UnitDrivenDecember2009;
                case UnitTestProviderType.Xunit:
                    return XapHostType.XunitContribApril2011;
                    case UnitTestProviderType.UnitDriven:
                        return XapHostType.UnitDrivenDecember2009;

                    case UnitTestProviderType.Undefined:
                    default:
                        throwNotSupportedException();
                        break;
                }
            }

            return XapHostType.UnitDrivenDecember2009;
        }
    }
}
