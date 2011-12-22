
namespace StatLight.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using StatLight.Core.Common;
    using StatLight.Core.WebBrowser;
    using StatLight.Core.WebServer.AssemblyResolution;
    using StatLight.Core.WebServer.XapHost;
    using StatLight.Core.WebServer.XapInspection;

    public class StatLightConfigurationFactory
    {
        public const int DefaultDialogSmackDownElapseMilliseconds = 5000;
        private readonly ILogger _logger;
        private readonly InputOptions _options;
        private readonly XapHostFileLoaderFactory _xapHostFileLoaderFactory;
        public StatLightConfigurationFactory(ILogger logger)
            : this(logger, new XapHostFileLoaderFactory(logger))
        {
        }

        public StatLightConfigurationFactory(ILogger logger, InputOptions options)
        {
            _logger = logger;
            _options = options;
            _xapHostFileLoaderFactory = new XapHostFileLoaderFactory(_logger);
        }

        public IEnumerable<StatLightConfiguration> GetConfigurations()
        {
            foreach (var xapPath in _options.XapPaths)
            {
                _logger.Debug("Starting configuration for: {0}".FormatWith(xapPath));
                yield return GetStatLightConfigurationForXap(xapPath);
            }

            foreach (var dllPath in _options.DllPaths)
            {
                _logger.Debug("Starting configuration for: {0}".FormatWith(dllPath));
                yield return GetStatLightConfigurationForDll(dllPath);
            }
        }

        private StatLightConfiguration GetStatLightConfigurationForXap(string xapPath)
        {

            Func<IEnumerable<ITestFile>> filesToCopyIntoHostXap = () => new List<ITestFile>();
            string runtimeVersion = null;
            IEnumerable<string> testAssemblyFormalNames = new List<string>();
            string entryPointAssembly = string.Empty;

            var xapReader = new XapReader(_logger);

            TestFileCollection testFileCollection = xapReader.LoadXapUnderTest(xapPath);
            runtimeVersion = XapReader.GetRuntimeVersion(xapPath);

            UnitTestProviderType unitTestProviderType = _options.UnitTestProviderType;
            MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion = _options.MicrosoftTestingFrameworkVersion;

            SetupUnitTestProviderType(testFileCollection, ref unitTestProviderType, ref microsoftTestingFrameworkVersion);

            entryPointAssembly = testFileCollection.TestAssemblyFullName;

            }
            filesToCopyIntoHostXap = () =>
            {
                return xapReader.LoadXapUnderTest(xapPath).FilesContainedWithinXap;
            };
                testAssemblyFormalNames = testFileCollection.GetAssemblyNames();
            }

            var clientConfig = new ClientTestRunConfiguration(unitTestProviderType, _options.MethodsToTest, _options.TagFilters, _options.NumberOfBrowserHosts, _options.WebBrowserType, entryPointAssembly, _options.WindowGeometry, testAssemblyFormalNames);

            var serverConfig = CreateServerConfiguration(
                xapPath,
                unitTestProviderType,
                microsoftTestingFrameworkVersion,
                filesToCopyIntoHostXap,
                _options.QueryString,
                _options.ForceBrowserStart,
                _options.WindowGeometry,
                runtimeVersion,
                isPhoneRun);

            return new StatLightConfiguration(clientConfig, serverConfig);
        }

        private StatLightConfiguration GetStatLightConfigurationForDll(string dllPath)
        {
            Func<IEnumerable<ITestFile>> filesToCopyIntoHostXap = () => new List<ITestFile>();
            string entryPointAssembly = string.Empty;
            string runtimeVersion = null;
            IEnumerable<string> testAssemblyFormalNames = new List<string>();

            var dllFileInfo = new FileInfo(dllPath);
            var assemblyResolver = new AssemblyResolver(_logger);
            var dependentAssemblies = assemblyResolver.ResolveAllDependentAssemblies(dllFileInfo.FullName);

            var coreFileUnderTest = new TestFile(dllFileInfo.FullName);
            var dependentFilesUnderTest = dependentAssemblies.Select(file => new TestFile(file)).ToList();
            dependentFilesUnderTest.Add(coreFileUnderTest);
            var xapReadItems = new TestFileCollection(_logger,
                                                        AssemblyName.GetAssemblyName(dllFileInfo.FullName).ToString(),
                                                        dependentFilesUnderTest);

            UnitTestProviderType unitTestProviderType = _options.UnitTestProviderType;
            MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion = _options.MicrosoftTestingFrameworkVersion;

            SetupUnitTestProviderType(xapReadItems, ref unitTestProviderType, ref microsoftTestingFrameworkVersion);

            entryPointAssembly = xapReadItems.TestAssemblyFullName;

            filesToCopyIntoHostXap = () =>
                                        {
                                            return new TestFileCollection(_logger,
                                                                    AssemblyName.GetAssemblyName(dllFileInfo.FullName)
                                                                        .ToString(),
                                                                    dependentFilesUnderTest).FilesContainedWithinXap;
                                        };

            var clientConfig = new ClientTestRunConfiguration(unitTestProviderType, _options.MethodsToTest, _options.TagFilters, _options.NumberOfBrowserHosts, _options.WebBrowserType, entryPointAssembly, _options.WindowGeometry, testAssemblyFormalNames);

            var serverConfig = CreateServerConfiguration(
                dllPath,
                clientConfig.UnitTestProviderType,
                microsoftTestingFrameworkVersion,
                filesToCopyIntoHostXap,
                _options.QueryString,
                _options.ForceBrowserStart,
                _options.WindowGeometry,
                runtimeVersion,
                isPhoneRun);

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

        private ServerTestRunConfiguration CreateServerConfiguration(
            string xapPath,
            UnitTestProviderType unitTestProviderType,
            MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion,
            Func<IEnumerable<ITestFile>> filesToCopyIntoHostXapFunc,
            string queryString,
            bool forceBrowserStart,
            WindowGeometry windowGeometry,
            string runtimeVersion,
            bool isPhoneRun)
        {
            XapHostType xapHostType = _xapHostFileLoaderFactory.MapToXapHostType(unitTestProviderType, microsoftTestingFrameworkVersion, isPhoneRun);

            Func<byte[]> hostXapFactory = () =>
            {
                byte[] hostXap = _xapHostFileLoaderFactory.LoadXapHostFor(xapHostType);
                hostXap = RewriteXapWithSpecialFiles(hostXap, filesToCopyIntoHostXapFunc, runtimeVersion);
                return hostXap;
            };

            return new ServerTestRunConfiguration(hostXapFactory, xapPath, xapHostType, queryString, forceBrowserStart, windowGeometry);
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
}
