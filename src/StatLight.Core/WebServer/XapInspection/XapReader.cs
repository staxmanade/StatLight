
using StatLight.Core.Configuration;

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
    using StatLight.Core.WebServer.XapHost;

    public class XapReader
    {
        private readonly ILogger _logger;

        public XapReader(ILogger logger)
        {
            _logger = logger;
        }

        public XapReadItems LoadXapUnderTest(string archiveFileName)
        {
            var xapItems = new XapReadItems();

            using (var archive = ZipFile.Read(archiveFileName))
            {
                var appManifest = LoadAppManifest(archive);

                if (appManifest != null)
                {
                    string testAssemblyName = GetTestAssemblyNameFromAppManifest(appManifest);

                    AssemblyName assemblyName = GetAssemblyName(archive, testAssemblyName);
                    if(assemblyName != null)
                    {
                    xapItems.TestAssemblyFullName = assemblyName.ToString();
                    }
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

            using (var sha = new SHA1Managed())
            {
                var result = sha.ComputeHash(bytes);
                foreach (byte outputByte in result)
                    // convert each byte to a Hexadecimal upper case string
                    encryptedString.Append(outputByte.ToString("x2").ToUpper());
                return encryptedString.ToString();
            }
        }

        private MicrosoftTestingFrameworkVersion? DetermineUnitTestVersion(ZipFile archive)
        {
            var incomingHash = (from zipEntry in archive
                                where FileNameCompare(zipEntry.FileName, "Microsoft.Silverlight.Testing.dll")
                                select SHA1Encryption(ReadFileIntoBytes(archive, zipEntry.FileName))).SingleOrDefault();
            if (incomingHash == null)
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

                // April SL 4 release
                new { Version = MicrosoftTestingFrameworkVersion.April2010, Hash = "357a677957f309ae85c3e5aeeda43a32bca23ad3", Supported = true, },

                // SL 3 build of the SL4 release to support phone
                new { Version = MicrosoftTestingFrameworkVersion.May2010, Hash = "de70e6249e6c13b60d8b556c6495b2d34a737d7c", Supported = true, },

                // Custom build of the framework - until and 'official' signed build is available
                new { Version = MicrosoftTestingFrameworkVersion.Feb2011, Hash = "8962C4BF1FFE3A2E432BC5991E2B142AFE1167A8", Supported = true, },
            };

            var foundVersion = definedVersions.Where(w => w.Hash.Equals(incomingHash, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            _logger.Debug("Incoming MSTest file's hash = {0}".FormatWith(incomingHash));

            if (foundVersion == null)
            {
                _logger.Warning(
                    "Could not determine the Microsoft testing framework version with a SHA1 hash of '{0}'"
                    .FormatWith(incomingHash));
                return null;
            }

            if (!foundVersion.Supported)
                throw new StatLightException("The Microsoft Silverlight Testing Framework from {0} is not supported in StatLight (anymore). Please look to upgrade to the latest version.".FormatWith(foundVersion.Version));

            return foundVersion.Version;
        }

        private static bool FileNameCompare(string fileA, string fileB)
        {
            return string.Equals(fileA, fileB, StringComparison.CurrentCultureIgnoreCase);
        }

        private static UnitTestProviderType DetermineUnitTestProviderType(ZipFile archive)
        {
            bool hasMSTest = false;

            foreach (ZipEntry zipEntry in archive)
            {
                // http://staxmanade.blogspot.com/2009/02/xunit-light-for-silverlight.html
                if (FileNameCompare(zipEntry.FileName, "XUnitLight.Silverlight.dll"))
                    return UnitTestProviderType.XUnit;

                //http://xunitcontrib.codeplex.com/
                if (zipEntry.FileName.ContainsIgnoreCase("xunitcontrib.runner.silverlight"))
                    return UnitTestProviderType.MSTestWithCustomProvider;

                if (zipEntry.FileName.ContainsIgnoreCase("unitdriven"))
                    return UnitTestProviderType.UnitDriven;

                if (zipEntry.FileName.ContainsIgnoreCase("nunit"))
                    return UnitTestProviderType.NUnit;

                if (FileNameCompare(zipEntry.FileName, "Microsoft.Silverlight.Testing.dll"))
                    hasMSTest = true;
            }

            if (hasMSTest)
                return UnitTestProviderType.MSTest;

            return UnitTestProviderType.Undefined;
        }

        private static AssemblyName GetAssemblyName(ZipFile zip1, string testAssemblyName)
        {
            string tempFileName = Path.GetTempFileName();
            var fileData = ReadFileIntoBytes(zip1, testAssemblyName);
            if (fileData != null)
            {
                File.WriteAllBytes(tempFileName, fileData);
                return AssemblyName.GetAssemblyName(tempFileName);
            }
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
