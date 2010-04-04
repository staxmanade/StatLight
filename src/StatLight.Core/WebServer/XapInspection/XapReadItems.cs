using System.Reflection;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer.XapHost;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapInspection
{
    public class XapReadItems
    {
        public MicrosoftTestingFrameworkVersion? MicrosoftSilverlightTestingFrameworkVersion;
        public UnitTestProviderType UnitTestProvider { get; set; }
        public string AppManifest { get; set; }
        public Assembly TestAssembly { get; set; }

        public void DebugWrite(ILogger logger)
        {
            logger.Debug("XapReadItems.UnitTestProvider = {0}".FormatWith(UnitTestProvider));
            logger.Debug("XapReadItems.MicrosoftSilverlightTestingFrameworkVersion = {0}".FormatWith(MicrosoftSilverlightTestingFrameworkVersion));
            logger.Debug("XapReadItems.TestAssembly = {0}".FormatWith(TestAssembly.FullName));
            
        }
    }
}