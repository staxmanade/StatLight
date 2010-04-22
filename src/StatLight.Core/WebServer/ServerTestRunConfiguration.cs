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

    public class ServerTestRunConfiguration
    {
        private readonly ILogger _logger;
        private readonly XapHostFileLoaderFactory _xapHostFileLoaderFactory;
        private readonly XapHostType _xapHostType;
        private readonly XapReadItems _xapReadItems;
        private byte[] _hostXap;
        public long DialogSmackDownElapseMilliseconds { get; set; }

        public ServerTestRunConfiguration(ILogger logger, XapHostFileLoaderFactory xapHostFileLoaderFactory, XapHostType xapHostType, XapReadItems xapReadItems)
        {
            _logger = logger;
            _xapHostFileLoaderFactory = xapHostFileLoaderFactory;
            _xapHostType = xapHostType;
            _xapReadItems = xapReadItems;

            DialogSmackDownElapseMilliseconds = 5000;
        }

        public byte[] HostXap
        {
            get
            {
                _hostXap = _xapHostFileLoaderFactory.LoadXapHostFor(_xapHostType);
                _hostXap = RewriteXapWithSpecialFiles(_hostXap);

                return _hostXap;
            }
        }

        private byte[] RewriteXapWithSpecialFiles(byte[] xapHost)
        {
            if (_xapReadItems != null)
            {
                //TODO: maybe specify this list as something passed in by the user???
                var specialFilesToCopyIntoHostXap = new List<string>
                                                        {
                                                            "ServiceReferences.ClientConfig",
                                                        };

                var filesToCopyIntoHostXap = (from x in _xapReadItems.FilesContianedWithinXap
                                              from specialFile in specialFilesToCopyIntoHostXap
                                              where x.FileName.Equals(specialFile, StringComparison.OrdinalIgnoreCase)
                                              select x).ToList();

                if (filesToCopyIntoHostXap.Any())
                {
                    xapHost = RewriteZipHostWithFiles(_hostXap, filesToCopyIntoHostXap);
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
}
