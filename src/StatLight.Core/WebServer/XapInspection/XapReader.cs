
namespace StatLight.Core.WebServer.XapInspection
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml.Linq;
    using Ionic.Zip;
    using StatLight.Core.Common;
    using StatLight.Core.UnitTestProviders;
    using StatLight.Core.WebServer.XapHost;

    public class XapReader
    {
        private readonly ILogger _logger;

        public XapReader(ILogger logger)
        {
            _logger = logger;
        }

        public XapReadItems GetTestAssembly(string archiveFileName)
        {
            var xapItems = new XapReadItems();

            using (var archive = ZipFile.Read(archiveFileName))
            {
                xapItems.AppManifest = LoadAppManifest(archive);

                if (xapItems.AppManifest != null)
                {
                    string testAssemblyName = GetTestAssemblyNameFromAppManifest(xapItems.AppManifest);

                    xapItems.TestAssembly = LoadTestAssembly(archive, testAssemblyName);
                }

                xapItems.UnitTestProvider = DetermineUnitTestProviderType(archive);

                xapItems.MicrosoftSilverlightTestingFrameworkVersion = DetermineUnitTestVersion(archive);

                xapItems.FilesContianedWithinXap = (from zipEntry in archive
                                                    let fileBytes = ReadFileIntoBytes(archive, zipEntry.FileName)
                                                    select new XapFile(zipEntry.FileName, fileBytes)).Cast<IXapFile>().ToList();

                foreach (var item in xapItems.FilesContianedWithinXap)
                    _logger.Debug("XapItems.FilesContainedWithinXap = {0}".FormatWith(item.FileName));
            }
            return xapItems;
        }

        private static string SHA1Encryption(byte[] bytes)
        {
            var encryptedString = new StringBuilder();

            var result = new SHA1Managed().ComputeHash(bytes);
            foreach (byte outputByte in result)
                // convert each byte to a Hexadecimal upper case string
                encryptedString.Append(outputByte.ToString("x2").ToUpper());
            return encryptedString.ToString();
        }

        private MicrosoftTestingFrameworkVersion? DetermineUnitTestVersion(ZipFile archive)
        {
            var incomingHash = (from zipEntry in archive
                                where fileNameCompare(zipEntry.FileName, "Microsoft.Silverlight.Testing.dll")
                                select SHA1Encryption(ReadFileIntoBytes(archive, zipEntry.FileName))).SingleOrDefault();
            if(incomingHash == null)
                return null;
            var definedVersions = new[]
            {
                /* Not supported anymore
                 * 
                 * Removed support because the [Timeout(?)] attribute didn't seem to work - 
                 * figured it was a good point to start deprecating some of these assemblies.
                 */
                new { Version = MicrosoftTestingFrameworkVersion.December2008, Hash = "9ecc2326c15db40aa28afc466a683279380affec", Supported = false, },
                new { Version = MicrosoftTestingFrameworkVersion.March2009, Hash = "8043c0da38fa18b224082e400189aca37ff0505f" , Supported = false, },

                // Still supported
                new { Version = MicrosoftTestingFrameworkVersion.July2009, Hash = "108d7c8a4f753f55433e1c195bb9e8f548bd627d", Supported = true, },
                new { Version = MicrosoftTestingFrameworkVersion.October2009, Hash = "8282f627299dc4cfd62f505ae7a6119aaae62d0d", Supported = true, },
                new { Version = MicrosoftTestingFrameworkVersion.November2009, Hash = "aba8d1ea91c37f06000b6f2a2927e4feb00bd97d", Supported = true, },

                // Mix 2010 Preview
                new { Version = MicrosoftTestingFrameworkVersion.March2010, Hash = "4b41678001f2000720a5b7479e4d20ea77820605", Supported = true, },
            };

            var foundVersion = definedVersions.Where(w => w.Hash.Equals(incomingHash, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            _logger.Debug("Incoming MSTest file's hash = {0}".FormatWith(incomingHash));

            if (foundVersion == null)
                return null;

            if (!foundVersion.Supported)
                throw new StatLightException("The Microsoft Silverlight Testing Framework from {0} is not supported in StatLight (anymore). Please look to upgrade to the latest version.".FormatWith(foundVersion.Version));

            return foundVersion.Version;
        }

        private bool fileNameCompare(string fileA, string fileB)
        {
            return string.Equals(fileA, fileB, StringComparison.CurrentCultureIgnoreCase);
        }

        private UnitTestProviderType DetermineUnitTestProviderType(ZipFile archive)
        {
            bool hasMSTest = false;

            foreach (ZipEntry zipEntry in archive)
            {
                if (fileNameCompare(zipEntry.FileName, "XUnitLight.Silverlight.dll"))
                    return UnitTestProviderType.XUnit;

                if (zipEntry.FileName.ToLower().Contains("unitdriven"))
                    return UnitTestProviderType.UnitDriven;

                if (zipEntry.FileName.ToLower().Contains("nunit"))
                    return UnitTestProviderType.NUnit;

                if (fileNameCompare(zipEntry.FileName, "Microsoft.Silverlight.Testing.dll"))
                    hasMSTest = true;
            }

            if (hasMSTest)
                return UnitTestProviderType.MSTest;

            return UnitTestProviderType.Undefined;
        }

        private static Assembly LoadTestAssembly(ZipFile zip1, string testAssemblyName)
        {
            var fileData = ReadFileIntoBytes(zip1, testAssemblyName);
            if (fileData != null)
                return Assembly.Load(fileData);
            return null;
        }

        private static string GetTestAssemblyNameFromAppManifest(string appManifest)
        {
            var root = XElement.Parse(appManifest);

            var entryPointAssemblyNode = root.Attribute("EntryPointAssembly");

            if (entryPointAssemblyNode == null)
                throw new StatLightException("Cannot find the EntryPointAssembly attribute in the AppManifest.xaml");

            return entryPointAssemblyNode.Value + ".dll";
        }

        private static string LoadAppManifest(ZipFile zip1)
        {
            var fileData = ReadFileIntoBytes(zip1, "AppManifest.xaml");
            if (fileData != null)
            {
                string xaml = Encoding.UTF8.GetString(fileData);
                if (xaml[0] == '<')
                    return xaml;
                return xaml.Substring(1);
            }

            return null;
        }

        private static byte[] ReadFileIntoBytes(ZipFile zipFile, string fileName)
        {
            var file = zipFile[fileName];
            if (file == null)
                return null;

            using (var stream = new MemoryStream())
            {
                file.Extract(stream);
                return stream.ToArray();
            }
        }
    }
}
