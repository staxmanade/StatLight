using System;

namespace StatLight.IntegrationTests.ProviderTests
{
    public class TestXapFileLocations
    {
        private static readonly string CurrentDirectory = Environment.CurrentDirectory + @"\\ProviderTests\_TestXaps\";

		public static string MSTest = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.MSTest.xap";
        public static string MSTestSL3 = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.MSTest-SL3.xap";
        public static string NUnit = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.NUnit.xap";
        public static string UnitDriven = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.UnitDriven.xap";
		public static string UnitDrivenSl3 =  CurrentDirectory + "StatLight.IntegrationTests.Silverlight.UnitDriven-SL3.xap";
        public static string XUnit = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.Xunit.xap";
        public static string XUnitSL3 = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.Xunit-SL3.xap";
        public static string XUnitLight = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.XunitLight.xap";
        public static string SilverlightIntegrationTests = CurrentDirectory + "StatLight.IntegrationTests.Silverlight.xap";
        public static string WinPhone = CurrentDirectory + "StatLight.IntegrationTests.Phone.MSTest.xap";

    }
}
