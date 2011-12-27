namespace StatLight.Core.WebServer.XapHost
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using StatLight.Core.Common;
    using StatLight.Core.Configuration;

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
                var e = (XapHostType)Enum.Parse(typeof(XapHostType), enumItemCasted.ToString());
                XapHostFileLoaders.Add(e, new DiskXapHostFileLoader(_logger, xapDirectory, ClientXapNameFormat.FormatWith(enumItemCasted)));
            }

            XapHostFileLoaders.Add(XapHostType.MSTest2010MayPhone, new DiskXapHostFileLoader(_logger, xapDirectory, ClientXapNameFormat.FormatWith(XapHostType.MSTest2010MayPhone)));
            XapHostFileLoaders.Add(XapHostType.UnitDriven2009December, new DiskXapHostFileLoader(_logger, xapDirectory, ClientXapNameFormat.FormatWith(XapHostType.UnitDriven2009December)));
            XapHostFileLoaders.Add(XapHostType.XunitContrib2011April, new DiskXapHostFileLoader(_logger, xapDirectory, ClientXapNameFormat.FormatWith(XapHostType.XunitContrib2011April)));
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
                    case UnitTestProviderType.MSTest:
                        if (microsoftTestingFrameworkVersion != null && microsoftTestingFrameworkVersion == MicrosoftTestingFrameworkVersion.MSTest2010May)
                            return XapHostType.MSTest2010MayPhone;

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
                    case UnitTestProviderType.XUnitLight:
                        return XapHostType.MSTest2010May;

                    case UnitTestProviderType.MSTestWithCustomProvider:
                    case UnitTestProviderType.MSTest:

                        if (microsoftTestingFrameworkVersion.HasValue)
                        {
                            var msTestVersionXapHostStringName = microsoftTestingFrameworkVersion.Value.ToString();

                            if (Enum.IsDefined(typeof(XapHostType), msTestVersionXapHostStringName))
                                return (XapHostType)Enum.Parse(typeof(XapHostType), msTestVersionXapHostStringName);
                        }

                        throwNotSupportedException();
                        break;

                    case UnitTestProviderType.UnitDriven:
                        return XapHostType.UnitDriven2009December;
                    case UnitTestProviderType.XUnit:
                        return XapHostType.XunitContrib2011April;

                    case UnitTestProviderType.Undefined:
                    default:
                        throwNotSupportedException();
                        break;
                }
            }

            return XapHostType.UnitDriven2009December;
        }
    }
}
