using System;
using System.Collections.Generic;
using System.Linq;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.WebServer.XapInspection
{
    public class TestFileCollection
    {
        private readonly ILogger _logger;
        private readonly string _testAssemblyFullName;
        private readonly IEnumerable<ITestFile> _files;
        private MicrosoftTestingFrameworkVersion? _msTestVersion;


        public TestFileCollection(ILogger logger, string testAssemblyFullName, IEnumerable<ITestFile> files)
        {
            _logger = logger;
            _testAssemblyFullName = testAssemblyFullName;
            _files = files;
        }

        public MicrosoftTestingFrameworkVersion? MSTestVersion
        {
            get
            {
                return _msTestVersion ??
                (_msTestVersion = DetermineUnitTestVersion(this.FilesContianedWithinXap));
            }
            set { _msTestVersion = value; }
        }

        public string TestAssemblyFullName { get; private set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public IEnumerable<ITestFile> FilesContianedWithinXap { get { return _files; } }

        public UnitTestProviderType UnitTestProvider
        {
            get
            {
                return DetermineUnitTestProviderType(this.FilesContianedWithinXap);
            }
        }

        public void DebugWrite(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            logger.Debug("TestFileCollection.UnitTestProvider = {0}".FormatWith(UnitTestProvider));
            logger.Debug("TestFileCollection.MSTestVersion = {0}".FormatWith(MSTestVersion));
            logger.Debug("TestFileCollection.TestAssembly = {0}".FormatWith(TestAssemblyFullName));

        }


        private MicrosoftTestingFrameworkVersion? DetermineUnitTestVersion(IEnumerable<ITestFile> files)
        {
            var incomingHash = (from xapFile in files
                                where xapFile.FileName.Equals("Microsoft.Silverlight.Testing.dll",StringComparison.OrdinalIgnoreCase)
                                select xapFile.File.Hash()).SingleOrDefault();

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

        private static UnitTestProviderType DetermineUnitTestProviderType(IEnumerable<ITestFile> files)
        {
            bool hasMSTest = false;

            foreach (var zipEntry in files)
            {
                // http://staxmanade.blogspot.com/2009/02/xunit-light-for-silverlight.html
                if (zipEntry.FileName.Equals("XUnitLight.Silverlight.dll", StringComparison.OrdinalIgnoreCase))
                    return UnitTestProviderType.XUnit;

                //http://xunitcontrib.codeplex.com/
                if (zipEntry.FileName.ContainsIgnoreCase("xunitcontrib.runner.silverlight"))
                    return UnitTestProviderType.MSTestWithCustomProvider;

                if (zipEntry.FileName.ContainsIgnoreCase("unitdriven"))
                    return UnitTestProviderType.UnitDriven;

                if (zipEntry.FileName.ContainsIgnoreCase("nunit"))
                    return UnitTestProviderType.NUnit;

                if (zipEntry.FileName.Equals("Microsoft.Silverlight.Testing.dll", StringComparison.OrdinalIgnoreCase))
                    hasMSTest = true;
            }

            if (hasMSTest)
                return UnitTestProviderType.MSTest;

            return UnitTestProviderType.Undefined;
        }
    }
}