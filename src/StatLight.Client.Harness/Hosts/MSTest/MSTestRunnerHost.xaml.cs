using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.Harness.Service;
using Microsoft.Silverlight.Testing.UI;
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.UI;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.MSTest;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.UnitDriven;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.Xunit;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;

namespace StatLight.Client.Harness.Hosts.MSTest
{
    public class MSTestRunnerHost : ITestRunnerHost
    {
        private readonly UnitTestSettings _settings = new UnitTestSettings();
        private ClientTestRunConfiguration _clientTestRunConfiguration;

        public void ConfigureWithClientTestRunConfiguration(ClientTestRunConfiguration clientTestRunConfiguration)
        {
            _clientTestRunConfiguration = clientTestRunConfiguration;

            _settings.TagExpression = _clientTestRunConfiguration.TagFilter;
            _settings.LogProviders.Add(new WebpageHeaderLogProvider("StatLight filters[{0}]".FormatWith(_clientTestRunConfiguration.TagFilter)));
        }

        public void ConfigureWithLoadedXapData(LoadedXapData loadedXapData)
        {
            // The below setup pretty much the standard setup used in the Microsoft testing framework
            _settings.TestHarness = new UnitTestHarness();
            _settings.TestService = new SilverlightTestService();
            _settings.LogProviders.Add(new UnitTestWebpageLog());
            _settings.LogProviders.Add(new TextFailuresLogProvider());

            // Below is the custom stuff...
            _settings.LogProviders.Add(new ServerHandlingLogProvider());
            foreach (var assembly in loadedXapData.TestAssemblies)
            {
                _settings.TestAssemblies.Add(assembly);
            }
            _settings.TestHarness.TestHarnessCompleted += CurrentHarness_TestHarnessCompleted;
        }

        public UIElement StartRun()
        {
            SetupUnitTestProvider(_clientTestRunConfiguration.UnitTestProviderType);

            return UnitTestSystem.CreateTestPage(_settings);
        }

        private static void CurrentHarness_TestHarnessCompleted(object sender, TestHarnessCompletedEventArgs e)
        {
            Server.SignalTestComplete(e.State);
        }

        private static void SetupUnitTestProvider(UnitTestProviderType unitTestProviderType)
        {
            Microsoft.Silverlight.Testing.UnitTesting.Metadata.UnitTestProviders.Providers.Clear();
            if (unitTestProviderType == UnitTestProviderType.XUnit)
            {
                UnitTestSystem.RegisterUnitTestProvider(new XUnitTestProvider());
            }
            else if (unitTestProviderType == UnitTestProviderType.NUnit)
            {
                UnitTestSystem.RegisterUnitTestProvider(new NUnitTestProvider());
            }
            else if (unitTestProviderType == UnitTestProviderType.UnitDriven)
            {
                UnitTestSystem.RegisterUnitTestProvider(new UnitDrivenTestProvider());
            }
            else
            {
                UnitTestSystem.RegisterUnitTestProvider(new VsttProvider());
            }
        }
    }
}