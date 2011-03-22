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

        public ZipFile RewriteZipHostWithFiles(byte[] hostXap, IEnumerable<ITestFile> filesToPlaceIntoHostXap)
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

                _logger.Debug("    add -  {0}".FormatWith(file.FileName));

                AddFile(zipFile, file, parts);
            }

            //NOTE: the StatLightTempName is a crazy string hick because I couldn't figure out how to get the XAttribute to look like x:Name=...

            zipFile.RemoveEntry("AppManifest.xaml");
            string manifestRewritten = xAppManifest.ToString().Replace("StatLightTempName", "x:Name").Replace(" xmlns=\"\"", string.Empty);
            zipFile.AddEntry("AppManifest.xaml", "/", manifestRewritten);

            return zipFile;
        }

        private void AddFile(ZipFile zipFile, ITestFile file, XElement parts)
        {
            AddFileToZip(zipFile, file.FileName, file.File);

            if ((Path.GetExtension(file.FileName) ?? string.Empty).Equals(".dll", StringComparison.OrdinalIgnoreCase))
            {
                /*
                //TODO: at a possible later time - understand if the pdb files have 
                // value in a silverlight xap and how to truly leverage them.

                var pdbName = Path.Combine(Path.GetDirectoryName(file.FileName), Path.GetFileNameWithoutExtension(file.FileName), ".pdb");
                if (File.Exists(pdbName))
                {
                    AddFileToZip(zipFile, pdbName, File.ReadAllBytes(pdbName));
                    _logger.Debug("        including pdb - {0}".FormatWith(pdbName));
                }
                */

                var name = Path.GetFileNameWithoutExtension(file.FileName);

                if (string.IsNullOrEmpty(Path.GetDirectoryName(file.FileName)))
                {
                    parts.Add(new XElement("AssemblyPart",
                                           new XAttribute("StatLightTempName", name),
                                           new XAttribute("Source", file.FileName)));

                    _logger.Debug("        updateed AppManifest - {0}".FormatWith(name));
                }
                else
                {
                    _logger.Debug("        Assembly not at root - not adding to AppManifest - {0}".FormatWith(name));
                }


            }
        }

        private static void AddFileToZip(ZipFile zipFile, string fileName, byte[] fileBytes)
        {
            if (Path.IsPathRooted(fileName))
            {
                zipFile.AddEntry(fileName, "/", fileBytes);
            }
            else
            {
                zipFile.AddEntry(Path.GetFileName(fileName), Path.GetDirectoryName(fileName), fileBytes);
            }
        }
    }
}
