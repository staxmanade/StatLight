using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
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

        public IZipArchive RewriteZipHostWithFiles(byte[] hostXap, IEnumerable<ITestFile> filesToPlaceIntoHostXap, string runtimeVersion)
        {
            if (filesToPlaceIntoHostXap == null) throw new ArgumentNullException("filesToPlaceIntoHostXap");

            //TODO: Write better tests and clean up the below
            // It's adding assemblies and other content, and re-writing the AppManifest.xaml

            var zipArchive = ZipArchiveFactory.Create(hostXap);

            var xAppManifest = zipArchive.GetAppManifest();

            var parts = xAppManifest.Elements().First();

            _logger.Debug("re-writing host xap with the following files");
            foreach (var file in filesToPlaceIntoHostXap)
            {
                if (zipArchive.ContainsFile(file.FileName))
                {
                    _logger.Debug("    -  already has file {0}".FormatWith(file.FileName));
                    continue;
                }

                _logger.Debug("    add -  {0}".FormatWith(file.FileName));

                AddFile(zipArchive, file, parts);
            }

            //NOTE: the StatLightTempName is a crazy string hick because I couldn't figure out how to get the XAttribute to look like x:Name=...

            //zipArchive.RemoveEntry("AppManifest.xaml");
            if (runtimeVersion != null)
                xAppManifest.SetAttributeValue("RuntimeVersion", runtimeVersion);
            string manifestRewritten = xAppManifest.ToString().Replace("StatLightTempName", "x:Name").Replace(" xmlns=\"\"", string.Empty);

            zipArchive.AddFile("AppManifest.xaml", manifestRewritten.ToByteArray());

            return zipArchive;
        }

        private void AddFile(IZipArchive zipArchive, ITestFile file, XElement parts)
        {
            AddFileInternal(zipArchive, file.FileName, file.File);

            if ((Path.GetExtension(file.FileName) ?? string.Empty).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                var name = Path.GetFileNameWithoutExtension(file.FileName);

                if (string.IsNullOrEmpty(Path.GetDirectoryName(file.FileName)))
                {
                    parts.Add(new XElement("AssemblyPart",
                                           new XAttribute("StatLightTempName", name),
                                           new XAttribute("Source", file.FileName)));

                    _logger.Debug("        updated AppManifest - {0}".FormatWith(name));
                }
                else
                {
                    _logger.Debug("        Assembly not at root - not adding to AppManifest - {0}".FormatWith(name));
                }
            }
        }

        internal static void AddFileInternal(IZipArchive zipArchive, string fileName, byte[] value)
        {
            if (zipArchive == null) throw new ArgumentNullException("zipArchive");
            if (fileName == null) throw new ArgumentNullException("fileName");
            if (value == null) throw new ArgumentNullException("value");

            zipArchive.AddFile(fileName, value);
        }
    }
}
