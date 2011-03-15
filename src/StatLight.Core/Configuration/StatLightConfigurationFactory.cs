
namespace StatLight.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;
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

        public StatLightConfiguration GetStatLightConfigurationForXap(UnitTestProviderType unitTestProviderType, string xapPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, bool showTestingBrowserHost)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");

            IEnumerable<IXapFile> filesToCopyIntoHostXap = new List<IXapFile>();
            string entryPointAssembly = string.Empty;
            if (isRemoteRun)
            {
            }
            else
            {
                AssertFileExists(xapPath);

                var xapReader = new XapReader(_logger);

                XapReadItems xapReadItems = xapReader.LoadXapUnderTest(xapPath);

                SetupUnitTestProviderType(xapReadItems, ref unitTestProviderType, ref microsoftTestingFrameworkVersion);

                entryPointAssembly = xapReadItems.TestAssemblyFullName;

                filesToCopyIntoHostXap = xapReadItems.FilesContianedWithinXap;
            }

            var clientConfig = new ClientTestRunConfiguration(unitTestProviderType, methodsToTest, tagFilters, numberOfBrowserHosts, webBrowserType, showTestingBrowserHost, entryPointAssembly);

            var serverConfig = CreateServerConfiguration(
                xapPath,
                clientConfig.UnitTestProviderType,
                microsoftTestingFrameworkVersion,
                filesToCopyIntoHostXap,
                DefaultDialogSmackDownElapseMilliseconds,
                queryString,
                forceBrowserStart,
                showTestingBrowserHost);

            return new StatLightConfiguration(clientConfig, serverConfig);
        }

        public StatLightConfiguration GetStatLightConfigurationForDll(UnitTestProviderType unitTestProviderType, string dllPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, bool showTestingBrowserHost)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");

            IEnumerable<IXapFile> filesToCopyIntoHostXap = new List<IXapFile>();
            string entryPointAssembly = string.Empty;
            if (isRemoteRun)
            {
            }
            else
            {
                AssertFileExists(dllPath);

                var dllFileInfo = new FileInfo(dllPath);

                var assemblyResolver = new AssemblyResolver(_logger, dllFileInfo.Directory);
                var dependentAssemblies = assemblyResolver.ResolveAllDependentAssemblies(dllFileInfo.FullName);

                var coreFileUnderTest = new XapFile(dllFileInfo.FullName);
                var dependentFilesUnderTest = dependentAssemblies.Select(file => new XapFile(file)).ToList();
                dependentFilesUnderTest.Add(coreFileUnderTest);

                var xapReadItems = new XapReadItems(_logger)
                {
                    FilesContianedWithinXap = new List<IXapFile>(dependentFilesUnderTest),
                    TestAssemblyFullName = AssemblyName.GetAssemblyName(dllFileInfo.FullName).ToString(),
                };

                SetupUnitTestProviderType(xapReadItems, ref unitTestProviderType, ref microsoftTestingFrameworkVersion);

                entryPointAssembly = xapReadItems.TestAssemblyFullName;

                filesToCopyIntoHostXap = xapReadItems.FilesContianedWithinXap;
            }

            var clientConfig = new ClientTestRunConfiguration(unitTestProviderType, methodsToTest, tagFilters, numberOfBrowserHosts, webBrowserType, showTestingBrowserHost, entryPointAssembly);

            var serverConfig = CreateServerConfiguration(
                dllPath,
                clientConfig.UnitTestProviderType,
                microsoftTestingFrameworkVersion,
                filesToCopyIntoHostXap,
                DefaultDialogSmackDownElapseMilliseconds,
                queryString,
                forceBrowserStart,
                showTestingBrowserHost);

            return new StatLightConfiguration(clientConfig, serverConfig);
        }




        private static void SetupUnitTestProviderType(XapReadItems xapReadItems, ref UnitTestProviderType unitTestProviderType, ref MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion)
        {
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
                    microsoftTestingFrameworkVersion = xapReadItems.MSTestVersion;
                }
            }
        }

        private static void AssertFileExists(string xapPath)
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
            IEnumerable<IXapFile> filesToCopyIntoHostXap,
            long dialogSmackDownElapseMilliseconds,
            string queryString,
            bool forceBrowserStart,
            bool showTestingBrowserHost)
        {
            XapHostType xapHostType = _xapHostFileLoaderFactory.MapToXapHostType(unitTestProviderType, microsoftTestingFrameworkVersion);

            Func<byte[]> hostXapFactory = () =>
            {
                byte[] hostXap = _xapHostFileLoaderFactory.LoadXapHostFor(xapHostType);
                hostXap = RewriteXapWithSpecialFiles(hostXap, filesToCopyIntoHostXap);
                return hostXap;
            };

            return new ServerTestRunConfiguration(hostXapFactory, dialogSmackDownElapseMilliseconds, xapPath, xapHostType, queryString, forceBrowserStart, showTestingBrowserHost);
        }

        private byte[] RewriteXapWithSpecialFiles(byte[] xapHost, IEnumerable<IXapFile> filesToCopyIntoHostXap)
        {
            if (filesToCopyIntoHostXap.Any())
            {
                var rewriter = new XapRewriter(_logger);

                xapHost = rewriter.RewriteZipHostWithFiles(xapHost, filesToCopyIntoHostXap)
                                .ToByteArray();
            }

            return xapHost;
        }


    }

    public interface IStatLightConfigurationFactory
    {
        StatLightConfiguration GetStatLightConfigurationForXap(UnitTestProviderType unitTestProviderType, string xapPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, bool showTestingBrowserHost);

        StatLightConfiguration GetStatLightConfigurationForDll(UnitTestProviderType unitTestProviderType, string dllPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, bool showTestingBrowserHost);
    }
}