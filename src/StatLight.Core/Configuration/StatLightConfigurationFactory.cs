
namespace StatLight.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using StatLight.Core.Common;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer.XapHost;
    using StatLight.Core.WebServer.XapInspection;

    public class StatLightConfigurationFactory : IStatLightConfigurationFactory
    {
        public const int DefaultDialogSmackDownElapseMilliseconds = 5000;
        private readonly ILogger _logger;
        private readonly XapHostFileLoaderFactory _xapHostFileLoaderFactory;

        public StatLightConfigurationFactory(ILogger logger)
        {
            _logger = logger;
            _xapHostFileLoaderFactory = new XapHostFileLoaderFactory(_logger);
        }

        public StatLightConfiguration GetStatLightConfiguration(UnitTestProviderType unitTestProviderType, string xapPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, bool showTestingBrowserHost)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");

            XapReadItems xapReadItems = null;
            string entryPointAssembly = string.Empty;
            if (isRemoteRun)
            {
            }
            else
            {
                AssertXapToTestFileExists(xapPath);

                xapReadItems = new XapReader(_logger).GetTestAssembly(xapPath);
                if (unitTestProviderType == UnitTestProviderType.Undefined || microsoftTestingFrameworkVersion == null)
                {
                    //TODO: Print message telling the user what the type is - and if they give it
                    // we don't have to "reflect" on the xap to determine the test provider type.

                    if (unitTestProviderType == UnitTestProviderType.Undefined)
                    {
                        unitTestProviderType = xapReadItems.UnitTestProvider;
                    }

                    if (
                        (xapReadItems.UnitTestProvider == UnitTestProviderType.MSTest ||
                            unitTestProviderType == UnitTestProviderType.MSTest ||
                            unitTestProviderType == UnitTestProviderType.MSTestWithCustomProvider)
                        && microsoftTestingFrameworkVersion == null)
                    {
                        microsoftTestingFrameworkVersion = xapReadItems.MicrosoftSilverlightTestingFrameworkVersion;
                    }
                }

                entryPointAssembly = xapReadItems.TestAssembly.FullName;
            }

            var clientConfig = new ClientTestRunConfiguration(unitTestProviderType, methodsToTest, tagFilters, numberOfBrowserHosts, webBrowserType, showTestingBrowserHost, entryPointAssembly);

            var serverConfig = CreateServerConfiguration(
                xapPath,
                clientConfig.UnitTestProviderType,
                microsoftTestingFrameworkVersion,
                xapReadItems,
                DefaultDialogSmackDownElapseMilliseconds,
                queryString,
                forceBrowserStart,
                showTestingBrowserHost);

            return new StatLightConfiguration(clientConfig, serverConfig);
        }

        private static void AssertXapToTestFileExists(string xapPath)
        {
            if (!File.Exists(xapPath))
            {
                throw new FileNotFoundException("File could not be found. [{0}]".FormatWith(xapPath));
            }
        }

        private ServerTestRunConfiguration CreateServerConfiguration(
            string xapPath,
            UnitTestProviderType unitTestProviderType,
            MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion,
            XapReadItems xapReadItems,
            long dialogSmackDownElapseMilliseconds,
            string queryString,
            bool forceBrowserStart,
            bool showTestingBrowserHost)
        {
            XapHostType xapHostType = _xapHostFileLoaderFactory.MapToXapHostType(unitTestProviderType, microsoftTestingFrameworkVersion);

            Func<byte[]> hostXapFactory = () =>
                                      {
                                          byte[] hostXap = _xapHostFileLoaderFactory.LoadXapHostFor(xapHostType);

                                          if (xapReadItems != null)
                                          {
                                              hostXap = RewriteXapWithSpecialFiles(hostXap, xapReadItems);
                                          }
                                          return hostXap;
                                      };
            return new ServerTestRunConfiguration(hostXapFactory, dialogSmackDownElapseMilliseconds, xapPath, xapHostType, queryString, forceBrowserStart, showTestingBrowserHost);
        }
        private byte[] RewriteXapWithSpecialFiles(byte[] xapHost, XapReadItems xapReadItems)
        {
            if (xapReadItems != null)
            {
                List<IXapFile> filesToCopyIntoHostXap = (from x in xapReadItems.FilesContianedWithinXap
                                                         select x).ToList();

                if (filesToCopyIntoHostXap.Any())
                {
                    var rewriter = new XapRewriter(_logger);
                    xapHost = rewriter.RewriteZipHostWithFiles(xapHost, filesToCopyIntoHostXap).ToByteArray();
                }
            }

            return xapHost;
        }

        
    }

    public interface IStatLightConfigurationFactory
    {
        StatLightConfiguration GetStatLightConfiguration(UnitTestProviderType unitTestProviderType, string xapPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, bool showTestingBrowserHost);
    }
}