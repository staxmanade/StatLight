using System.ComponentModel.Composition;
using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using StatLight.Client.Harness.Events;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.MSTest;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.UnitDriven;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.Xunit;
using StatLight.Core.UnitTestProviders;
using StatLight.Core.WebServer;

namespace StatLight.Client.Harness.Hosts.MSTest
{
    [Export(typeof(ITestRunnerHost))]
    public class MSTestRunnerHost : ITestRunnerHost
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;
        private LoadedXapData _loadedXapData;

        public void ConfigureWithClientTestRunConfiguration(ClientTestRunConfiguration clientTestRunConfiguration)
        {
            _clientTestRunConfiguration = clientTestRunConfiguration;
        }

        public void ConfigureWithLoadedXapData(LoadedXapData loadedXapData)
        {
            _loadedXapData = loadedXapData;
        }

        public UIElement StartRun()
        {
            SetupUnitTestProvider(_clientTestRunConfiguration.UnitTestProviderType);
            
            var settings = ConfigureSettings();

            return UnitTestSystem.CreateTestPage(settings);
        }

        private static void CurrentHarness_TestHarnessCompleted(object sender, TestHarnessCompletedEventArgs e)
        {
            var state = e.State;
            var signalTestCompleteClientEvent = new SignalTestCompleteClientEvent
            {
                Failed = state.Failed,
                TotalFailureCount = state.Failures,
                TotalTestsCount = state.TotalScenarios,
            };
            Server.SignalTestComplete(signalTestCompleteClientEvent);
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

        private UnitTestSettings ConfigureSettings()
        {
            var settings = UnitTestSystem.CreateDefaultSettings();

#if MSTestMarch2010
            settings.StartRunImmediately = true;
#else
#endif

            // Below is the custom stuff...
            settings.TagExpression = _clientTestRunConfiguration.TagFilter;
            settings.LogProviders.Add(new ServerHandlingLogProvider());
            foreach (var assembly in _loadedXapData.TestAssemblies)
            {
                settings.TestAssemblies.Add(assembly);
            }
            settings.TestHarness.TestHarnessCompleted += CurrentHarness_TestHarnessCompleted;
            return settings;
        }
    }
}
