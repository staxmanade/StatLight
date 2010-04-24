using StatLight.Core.UnitTestProviders;

namespace StatLight.Core.WebServer
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Ionic.Zip;
    using StatLight.Core.Common;
    using StatLight.Core.WebServer.XapHost;
    using StatLight.Core.WebServer.XapInspection;

    public class ServerTestRunConfigurationFactory
    {
        public const int DefaultDialogSmackDownElapseMilliseconds = 5000;

        private readonly ILogger _logger;
        private readonly XapHostFileLoaderFactory _xapHostFileLoaderFactory;

        public ServerTestRunConfigurationFactory(ILogger logger, XapHostFileLoaderFactory xapHostFileLoaderFactory)
        {
            _logger = logger;
            _xapHostFileLoaderFactory = xapHostFileLoaderFactory;
        }
        public ServerTestRunConfiguration CreateServerConfiguration(
            string xapPath,
            UnitTestProviderType unitTestProviderType,
            MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion,
            XapReadItems xapReadItems)
        {
            return CreateServerConfiguration(xapPath, unitTestProviderType, microsoftTestingFrameworkVersion, xapReadItems, DefaultDialogSmackDownElapseMilliseconds);
        }

        public ServerTestRunConfiguration CreateServerConfiguration(
            string xapPath,
            UnitTestProviderType unitTestProviderType,
            MicrosoftTestingFrameworkVersion? microsoftTestingFrameworkVersion,
            XapReadItems xapReadItems,
            long dialogSmackDownElapseMilliseconds
            )
        {
            XapHostType xapHostType = _xapHostFileLoaderFactory.MapToXapHostType(unitTestProviderType, microsoftTestingFrameworkVersion);

            var hostXap = _xapHostFileLoaderFactory.LoadXapHostFor(xapHostType);
            hostXap = RewriteXapWithSpecialFiles(hostXap, xapReadItems);

            return new ServerTestRunConfiguration(hostXap, dialogSmackDownElapseMilliseconds, xapPath);
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
                                              from specialFile in specialFilesToCopyIntoHostXap
                                              where x.FileName.Equals(specialFile, StringComparison.OrdinalIgnoreCase)
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
            ZipFile zipFile = ZipFile.Read(hostXap);

            _logger.Debug("re-writing host xap with the following files");
            foreach (var file in filesToPlaceIntoHostXap)
            {
                _logger.Debug("    -  {0}".FormatWith(file.FileName));
                zipFile.AddEntry(file.FileName, "/", file.File);
            }

            using (var stream = new MemoryStream())
            {
                zipFile.Save(stream);
                return stream.ToArray();
            }
        }
    }

    public class ServerTestRunConfiguration
    {
        public ServerTestRunConfiguration(byte[] xapHost, long dialogSmackDownElapseMilliseconds, string xapToTest)
        {
            HostXap = xapHost;
            DialogSmackDownElapseMilliseconds = dialogSmackDownElapseMilliseconds;
            XapToTestPath = xapToTest;
        }

        public long DialogSmackDownElapseMilliseconds { get; private set; }
        public byte[] HostXap { get; private set; }

        public string XapToTestPath { get; private set; }
    }
}
