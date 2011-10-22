
using System.Diagnostics;

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

        public StatLightConfiguration GetStatLightConfigurationForXap(UnitTestProviderType unitTestProviderType, string xapPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, WindowGeometry windowGeometry)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");
            if (windowGeometry == null)
                throw new ArgumentNullException("windowGeometry");

            if(windowGeometry.ShouldShowWindow && !Environment.UserInteractive)
                throw new StatLightException("You cannot use the -b option as your C.I. server's agent process is not running in desktop interactive mode.");

            Func<IEnumerable<ITestFile>> filesToCopyIntoHostXap = () => new List<ITestFile>();
            string runtimeVersion = null;
            string entryPointAssembly = string.Empty;
            if (isRemoteRun)
            {
            }
            else
            {
                AssertFileExists(xapPath);

                var xapReader = new XapReader(_logger);

                TestFileCollection testFileCollection = xapReader.LoadXapUnderTest(xapPath);
                runtimeVersion = XapReader.GetRuntimeVersion(xapPath);

                SetupUnitTestProviderType(testFileCollection, ref unitTestProviderType, ref microsoftTestingFrameworkVersion);

                entryPointAssembly = testFileCollection.TestAssemblyFullName;

                filesToCopyIntoHostXap = () =>
                {
                    return xapReader.LoadXapUnderTest(xapPath).FilesContainedWithinXap;
                };

            }

            var clientConfig = new ClientTestRunConfiguration(unitTestProviderType, methodsToTest, tagFilters, numberOfBrowserHosts, webBrowserType, entryPointAssembly, windowGeometry);

            var serverConfig = CreateServerConfiguration(
                xapPath,
                clientConfig.UnitTestProviderType,
                microsoftTestingFrameworkVersion,
                filesToCopyIntoHostXap,
                DefaultDialogSmackDownElapseMilliseconds,
                queryString,
                forceBrowserStart,
                windowGeometry,
                runtimeVersion);

            return new StatLightConfiguration(clientConfig, serverConfig);
        }

        public StatLightConfiguration GetStatLightConfigurationForDll(UnitTestProviderType unitTestProviderType, string dllPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, WindowGeometry windowGeometry)
        {
            if (queryString == null)
                throw new ArgumentNullException("queryString");

            Func<IEnumerable<ITestFile>> filesToCopyIntoHostXap = () => new List<ITestFile>();
            string entryPointAssembly = string.Empty;
            string runtimeVersion = null;
            if (isRemoteRun)
            {
            }
            else
            {
                AssertFileExists(dllPath);

                var dllFileInfo = new FileInfo(dllPath);
                var assemblyResolver = new AssemblyResolver(_logger);
                var dependentAssemblies = assemblyResolver.ResolveAllDependentAssemblies(dllFileInfo.FullName);

                var coreFileUnderTest = new TestFile(dllFileInfo.FullName);
                var dependentFilesUnderTest = dependentAssemblies.Select(file => new TestFile(file)).ToList();
                dependentFilesUnderTest.Add(coreFileUnderTest);
                var xapReadItems = new TestFileCollection(_logger,
                                                          AssemblyName.GetAssemblyName(dllFileInfo.FullName).ToString(),
                                                          dependentFilesUnderTest);

                SetupUnitTestProviderType(xapReadItems, ref unitTestProviderType, ref microsoftTestingFrameworkVersion);

                entryPointAssembly = xapReadItems.TestAssemblyFullName;

                filesToCopyIntoHostXap =()=>
                                            {
                                                return new TestFileCollection(_logger,
                                                                       AssemblyName.GetAssemblyName(dllFileInfo.FullName)
                                                                           .ToString(),
                                                                       dependentFilesUnderTest).FilesContainedWithinXap;
                                            };
            }

            var clientConfig = new ClientTestRunConfiguration(unitTestProviderType, methodsToTest, tagFilters, numberOfBrowserHosts, webBrowserType, entryPointAssembly, windowGeometry);

            var serverConfig = CreateServerConfiguration(
                dllPath,
                clientConfig.UnitTestProviderType,
                microsoftTestingFrameworkVersion,
                filesToCopyIntoHostXap,
                DefaultDialogSmackDownElapseMilliseconds,
                queryString,
                forceBrowserStart,
                windowGeometry,
                runtimeVersion);

            return new StatLightConfiguration(clientConfig, serverConfig);
        }




        private static void SetupUnitTestProviderType(TestFileCollection testFileCollection, ref UnitTestProviderType unitTestProviderType, ref MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion)
        {
            if (unitTestProviderType == UnitTestProviderType.Undefined || microsoftTestingFrameworkVersion == null)
            {
                //TODO: Print message telling the user what the type is - and if they give it
                // we don't have to "reflect" on the xap to determine the test provider type.

                if (unitTestProviderType == UnitTestProviderType.Undefined)
                {
                    unitTestProviderType = testFileCollection.UnitTestProvider;
                }

                if (
                    (testFileCollection.UnitTestProvider == UnitTestProviderType.MSTest ||
                     unitTestProviderType == UnitTestProviderType.MSTest ||
                     unitTestProviderType == UnitTestProviderType.MSTestWithCustomProvider)
                    && microsoftTestingFrameworkVersion == null)
                {
                    microsoftTestingFrameworkVersion = testFileCollection.MSTestVersion;
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
            Func<IEnumerable<ITestFile>> filesToCopyIntoHostXapFunc,
            long dialogSmackDownElapseMilliseconds,
            string queryString,
            bool forceBrowserStart,
            WindowGeometry windowGeometry,
            string runtimeVersion)
        {
            XapHostType xapHostType = _xapHostFileLoaderFactory.MapToXapHostType(unitTestProviderType, microsoftTestingFrameworkVersion);

            Func<byte[]> hostXapFactory = () =>
            {
                byte[] hostXap = _xapHostFileLoaderFactory.LoadXapHostFor(xapHostType);
                hostXap = RewriteXapWithSpecialFiles(hostXap, filesToCopyIntoHostXapFunc, runtimeVersion);
                return hostXap;
            };

            return new ServerTestRunConfiguration(hostXapFactory, dialogSmackDownElapseMilliseconds, xapPath, xapHostType, queryString, forceBrowserStart, windowGeometry);
        }

        private byte[] RewriteXapWithSpecialFiles(byte[] xapHost, Func<IEnumerable<ITestFile>> filesToCopyIntoHostXapFunc, string runtimeVersion)
        {
            var files = filesToCopyIntoHostXapFunc();
            if (files.Any())
            {
                var rewriter = new XapRewriter(_logger);

                xapHost = rewriter
                    .RewriteZipHostWithFiles(xapHost, files, runtimeVersion)
                    .ToByteArray();
            }

            return xapHost;
        }


    }

    public interface IStatLightConfigurationFactory
    {
        StatLightConfiguration GetStatLightConfigurationForXap(UnitTestProviderType unitTestProviderType, string xapPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, WindowGeometry windowGeometry);

        StatLightConfiguration GetStatLightConfigurationForDll(UnitTestProviderType unitTestProviderType, string dllPath, MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion, Collection<string> methodsToTest, string tagFilters, int numberOfBrowserHosts, bool isRemoteRun, string queryString, WebBrowserType webBrowserType, bool forceBrowserStart, WindowGeometry windowGeometry);
    }
}
