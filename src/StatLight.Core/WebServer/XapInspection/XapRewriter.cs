using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Ionic.Zip;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapInspection
{
    public class XapRewriter
    {
        private readonly ILogger _logger;

        public XapRewriter(ILogger logger)
        {
            _logger = logger;
        }

        public ZipFile RewriteZipHostWithFiles(byte[] hostXap, IEnumerable<IXapFile> filesToPlaceIntoHostXap)
        {
            if (filesToPlaceIntoHostXap == null) throw new ArgumentNullException("filesToPlaceIntoHostXap");

            //TODO: Write better tests and clean up the below
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

                if (file.FileName.IndexOf('\\') >= 1)
                {
                    zipFile.AddEntry(Path.GetFileName(file.FileName), Path.GetDirectoryName(file.FileName), file.File);
                }
                else
                {
                    zipFile.AddEntry(file.FileName, "/", file.File);
                }


                if ((Path.GetExtension(file.FileName) ?? string.Empty).Equals(".dll", StringComparison.OrdinalIgnoreCase))
                {
                    var name = Path.GetFileNameWithoutExtension(file.FileName);
                    parts.Add(new XElement("AssemblyPart",
                                           new XAttribute("StatLightTempName", name),
                                           new XAttribute("Source", file.FileName)));

                    _logger.Debug("    -  Updating AppManifest - {0}".FormatWith(name));

                }
            }

            zipFile.RemoveEntry("AppManifest.xaml");
            string manifestRewritten = xAppManifest.ToString().Replace("StatLightTempName", "x:Name").Replace(" xmlns=\"\"", string.Empty);
            zipFile.AddEntry("AppManifest.xaml", "/", manifestRewritten);

            return zipFile;
        }
    }
}
