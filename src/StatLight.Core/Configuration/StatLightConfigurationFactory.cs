
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Linq;
using StatLight.Core.WebBrowser;
using StatLight.Core.WebServer;

namespace StatLight.Core.Configuration
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ionic.Zip;
    using StatLight.Core.Common;
    using StatLight.Core.WebServer.XapHost;
    using StatLight.Core.WebServer.XapInspection;

    public class StatLightConfigurationFactory
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

            string xapUrl = null;
            XapReadItems xapReadItems = null;
            string entryPointAssembly = string.Empty;
            if (isRemoteRun)
            {
                xapUrl = xapPath;
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

            var clientConfig = new ClientTestRunConfiguration(unitTestProviderType, methodsToTest, tagFilters, numberOfBrowserHosts, xapUrl, webBrowserType, showTestingBrowserHost, entryPointAssembly);

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

            //TODO: remove this thing.
            Func<byte[]> xapToTestFactory = () => new byte[] { };

            Func<byte[]> hostXapFactory = () =>
                                      {
                                          byte[] hostXap = _xapHostFileLoaderFactory.LoadXapHostFor(xapHostType);

                                          if (xapReadItems != null)
                                          {
                                              hostXap = RewriteXapWithSpecialFiles(hostXap, xapReadItems);

                                              xapToTestFactory = () =>
                                                                     {
                                                                         AssertXapToTestFileExists(xapPath);
                                                                         _logger.Debug(
                                                                             "Loading XapToTest {0}".FormatWith(xapPath));
                                                                         return File.ReadAllBytes(xapPath);
                                                                     };
                                          }
                                          return hostXap;
                                      };
            return new ServerTestRunConfiguration(hostXapFactory, dialogSmackDownElapseMilliseconds, xapPath, xapHostType, xapToTestFactory, queryString, forceBrowserStart, showTestingBrowserHost);
        }
        private byte[] RewriteXapWithSpecialFiles(byte[] xapHost, XapReadItems xapReadItems)
        {
            if (xapReadItems != null)
            {
                //TODO: maybe specify this list as something passed in by the user???
                var specialFilesToCopyIntoHostXap = new List<string>
                                                        {
                                                            "ServiceReferences.ClientConfig",
                                                        };

                var filesToCopyIntoHostXap = (from x in xapReadItems.FilesContianedWithinXap
                                              //from specialFile in specialFilesToCopyIntoHostXap
                                              //where x.FileName.Equals(specialFile, StringComparison.OrdinalIgnoreCase)
                                              select x).ToList();

                if (filesToCopyIntoHostXap.Any())
                {
                    xapHost = RewriteZipHostWithFiles(xapHost, filesToCopyIntoHostXap);
                }
            }

            return xapHost;
        }

        private byte[] RewriteZipHostWithFiles(byte[] hostXap, IEnumerable<IXapFile> filesToPlaceIntoHostXap)
        {
            //TODO: Write tests and clean up the below
            // It's adding assemblies and other content, and re-writing the AppManifest.xaml

            ZipFile zipFile = ZipFile.Read(hostXap);

            ZipEntry appManifestEntry = zipFile["AppManifest.xaml"];
            var xAppManifest = XElement.Load(appManifestEntry.OpenReader());

            var parts = xAppManifest.Elements().First();

            _logger.Debug("re-writing host xap with the following files");
            foreach (var file in filesToPlaceIntoHostXap)
            {
                if (zipFile.EntryFileNames.Contains(file.FileName))
                {
                    _logger.Debug("    -  already has file {0}".FormatWith(file.FileName));
                    continue;
                }

                _logger.Debug("    -  {0}".FormatWith(file.FileName));
                zipFile.AddEntry(file.FileName, "/", file.File);

                if ((Path.GetExtension(file.FileName) ?? string.Empty).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    var name = Path.GetFileNameWithoutExtension(file.FileName);
                    XNamespace x = "x";
                    parts.Add(new XElement("AssemblyPart",
                                           new XAttribute("StatLightTempName", name),
                                           new XAttribute("Source", file.FileName)));

                    _logger.Debug("    -  Updating AppManifest - {0}".FormatWith(name));

                }
            }

            zipFile.RemoveEntry("AppManifest.xaml");
            string manifestRewritten = xAppManifest.ToString().Replace("StatLightTempName", "x:Name").Replace("xmlns=\"\"", string.Empty);
            zipFile.AddEntry("AppManifest.xaml", "/", manifestRewritten);

            using (var stream = new MemoryStream())
            {
                zipFile.Save(stream);
                return stream.ToArray();
            }
        }
    }
}