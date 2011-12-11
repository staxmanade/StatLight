using System;
using System.Collections.Generic;
using System.Linq;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.WebServer.XapHost;

namespace StatLight.Core.WebServer.XapInspection
{
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix")]
    public class TestFileCollection
    {
        private readonly ILogger _logger;
        private readonly IEnumerable<ITestFile> _files;
        private MicrosoftTestingFrameworkVersion? _msTestVersion;


        public TestFileCollection(ILogger logger, string testAssemblyFullName, IEnumerable<ITestFile> files)
        {
            _logger = logger;
            TestAssemblyFullName = testAssemblyFullName;
            _files = files;
        }

        public MicrosoftTestingFrameworkVersion? MSTestVersion
        {
            get
            {
                return _msTestVersion ??
                (_msTestVersion = DetermineUnitTestVersion(this.FilesContainedWithinXap));
            }
            set { _msTestVersion = value; }
        }

        public string TestAssemblyFullName { get; private set; }
        public IEnumerable<ITestFile> FilesContainedWithinXap { get { return _files; } }

        public UnitTestProviderType UnitTestProvider
        {
            get
            {
                return DetermineUnitTestProviderType(this.FilesContainedWithinXap);
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
            var definedVersions = new[]
            {
                /* Not supported anymore
                 * 
                 * Removed support because the [Timeout(?)] attribute didn't seem to work - 
                 * figured it was a good point to start deprecating some of these assemblies.
                 */
                //new { Version = MicrosoftTestingFrameworkVersion.MSTest2008December, MicrosoftSilverlightTestingHash = "9ecc2326c15db40aa28afc466a683279380affec", Supported = false, VisualStudioQualityHash = "279e346983bd33bce15462aea198d3afc70ecbf0", },
                //new { Version = MicrosoftTestingFrameworkVersion.MSTest2009March, MicrosoftSilverlightTestingHash = "8043c0da38fa18b224082e400189aca37ff0505f" , Supported = false, VisualStudioQualityHash = "9d8a5bdc59cb80eaf47a2a17353485c837ef817c", },

                // Still supported
                new { Version = MicrosoftTestingFrameworkVersion.MSTest2009July, MicrosoftSilverlightTestingHash = "108d7c8a4f753f55433e1c195bb9e8f548bd627d", Supported = true, VisualStudioQualityHash = "e8d46980845d785615c7687fe51fb85d141f5297", },
                new { Version = MicrosoftTestingFrameworkVersion.MSTest2009October, MicrosoftSilverlightTestingHash = "8282f627299dc4cfd62f505ae7a6119aaae62d0d", Supported = true, VisualStudioQualityHash = "cf41fb881d6485035ddfedc57a3ca07be101fb6f", },
                new { Version = MicrosoftTestingFrameworkVersion.MSTest2009November, MicrosoftSilverlightTestingHash = "aba8d1ea91c37f06000b6f2a2927e4feb00bd97d", Supported = true, VisualStudioQualityHash = "67f08086a0a7025928344820fbdd8ebdfba40179", },

                // Mix 2010 Preview
                new { Version = MicrosoftTestingFrameworkVersion.MSTest2010March, MicrosoftSilverlightTestingHash = "4b41678001f2000720a5b7479e4d20ea77820605", Supported = true, VisualStudioQualityHash = "221bab08a6c78e0d66f2bbf69f38c6be79d04c1f", },

                // April SL 4 release
                new { Version = MicrosoftTestingFrameworkVersion.MSTest2010April, MicrosoftSilverlightTestingHash = "357a677957f309ae85c3e5aeeda43a32bca23ad3", Supported = true, VisualStudioQualityHash = "26e01beb49ef84d79069da70edc6e5af0876a550", },

                // SL 3 build of the SL4 release to support phone
                new { Version = MicrosoftTestingFrameworkVersion.MSTest2010May, MicrosoftSilverlightTestingHash = "de70e6249e6c13b60d8b556c6495b2d34a737d7c", Supported = true, VisualStudioQualityHash = "4b0fedf528fa9f0fbdc1c0af875cabe3fe7956f6", },

                // Custom build of the framework - until and 'official' signed build is available
                new { Version = MicrosoftTestingFrameworkVersion.MSTest2011Feb, MicrosoftSilverlightTestingHash = "8962C4BF1FFE3A2E432BC5991E2B142AFE1167A8", Supported = true, VisualStudioQualityHash = "26e01beb49ef84d79069da70edc6e5af0876a550", },

                // Released on Jeff's blog
                // http://www.jeff.wilcox.name/2011/06/updated-ut-mango-bits/
                new { Version = MicrosoftTestingFrameworkVersion.MSTest2011June, MicrosoftSilverlightTestingHash = "b43f74adec6e911ce0e01d882fd2958a33f8c5fd", Supported = true, VisualStudioQualityHash = "303e7eb91b26dd6aad394dd4727351485068c8be", },
            };


            var incomingHash = GetFileHashIfExists(files, "Microsoft.Silverlight.Testing.dll");

            var foundVersionMSSLTF = definedVersions.Where(w => w.MicrosoftSilverlightTestingHash.Equals(incomingHash, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();

            if (foundVersionMSSLTF == null)
            {
                var incomingHashVSQ = GetFileHashIfExists(files, "Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll");

                // the next scenario comes up if you're trying to run against a silverlight test project DLL only and the Microsoft.Silverlight.Testing is not included as a "reference" to the output assemblies.
                // We fall back to the verson of Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll and map that to the correct Microsoft.Silverlight.Testing.
                var foundVersionVSQ = definedVersions.Where(w => w.VisualStudioQualityHash.Equals(incomingHashVSQ, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                if (foundVersionVSQ != null)
                    foundVersionMSSLTF = foundVersionVSQ;
            }

            if (foundVersionMSSLTF == null)
            {
                _logger.Warning("Could not determine the Microsoft testing framework version with a SHA1 hash of '{0}'"
                    .FormatWith(incomingHash));

                return null;
            }

            if (!foundVersionMSSLTF.Supported)
                throw new StatLightException("The Microsoft Silverlight Testing Framework from {0} is not supported in StatLight (anymore). Please look to upgrade to the latest version.".FormatWith(foundVersionMSSLTF.Version));

            return foundVersionMSSLTF.Version;
        }

        private string GetFileHashIfExists(IEnumerable<ITestFile> files, string fileName)
        {
            var hashFound = (from xapFile in files
                             where xapFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)
                             select xapFile.File.Hash()).SingleOrDefault();

            _logger.Debug("Incoming {0} file's hash = {1}".FormatWith(fileName, hashFound));

            return hashFound;
        }

        private static UnitTestProviderType DetermineUnitTestProviderType(IEnumerable<ITestFile> files)
        {
            bool hasMSTest = false;

            foreach (var zipEntry in files)
            {
                // http://staxmanade.blogspot.com/2009/02/xunit-light-for-silverlight.html
                if (zipEntry.FileName.Equals("XUnitLight.Silverlight.dll", StringComparison.OrdinalIgnoreCase))
                    return UnitTestProviderType.XUnitLight;

                //http://xunitcontrib.codeplex.com/
                if (zipEntry.FileName.ContainsIgnoreCase("xunitcontrib.runner.silverlight"))
                    return UnitTestProviderType.MSTestWithCustomProvider;

                if (zipEntry.FileName.ContainsIgnoreCase("xunit.runner.silverlight"))
                    return UnitTestProviderType.Xunit;

                if (zipEntry.FileName.ContainsIgnoreCase("unitdriven"))
                    return UnitTestProviderType.UnitDriven;

                if (zipEntry.FileName.ContainsIgnoreCase("nunit"))
                    return UnitTestProviderType.NUnit;

                if (zipEntry.FileName.Equals("Microsoft.Silverlight.Testing.dll", StringComparison.OrdinalIgnoreCase) ||
                    zipEntry.FileName.Equals("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll", StringComparison.OrdinalIgnoreCase))
                    hasMSTest = true;
            }

            if (hasMSTest)
                return UnitTestProviderType.MSTest;

            return UnitTestProviderType.Undefined;
        }
    }
}