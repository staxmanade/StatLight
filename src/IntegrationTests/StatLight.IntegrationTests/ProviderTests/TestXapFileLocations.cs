using System.IO;

namespace StatLight.IntegrationTests.ProviderTests
{
    public class TestXapFileLocations
    {
        private static string GetBaseDirectory()
        {
            var baseDirectoryName = Path.GetDirectoryName(typeof(TestXapFileLocations).Assembly.CodeBase);
            return Path.Combine(baseDirectoryName, @"ProviderTests", "_TestXaps").Replace(@"file:\", "") + "\\";
        }

        private static readonly string CurrentDirectory = GetBaseDirectory();
        public static string MSTestSL3 = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.MSTest-SL3.xap";
        public static string MSTestSL4 = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.MSTest-SL4.xap";
        public static string MSTestSL5 = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.MSTest-SL5.xap";
        public static string NUnit = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.NUnit.xap";
        public static string UnitDriven = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.UnitDriven.xap";
        public static string UnitDrivenSl3 = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.UnitDriven-SL3.xap";
        public static string XUnit = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.Xunit.xap";
        public static string XUnitSL3 = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.Xunit-SL3.xap";
        public static string XUnitLight = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.XunitLight.xap";
        public static string SilverlightIntegrationTests = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.xap";
    }
}