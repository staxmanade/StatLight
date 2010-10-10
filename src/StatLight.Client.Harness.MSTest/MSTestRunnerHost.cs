using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using StatLight.Client.Harness.Events;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.MSTest;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.UnitDriven;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.Xunit;
using StatLight.Client.Harness.Messaging;
using StatLight.Core.Configuration;

namespace StatLight.Client.Harness.Hosts.MSTest
{
    public class MSTestRunnerHost : ITestRunnerHost
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;
        private ILoadedXapData _loadedXapData;

        public void ConfigureWithClientTestRunConfiguration(ClientTestRunConfiguration clientTestRunConfiguration)
        {
            _clientTestRunConfiguration = clientTestRunConfiguration;
        }

        public void ConfigureWithLoadedXapData(ILoadedXapData loadedXapData)
        {
            _loadedXapData = loadedXapData;
        }

        public UIElement StartRun()
        {
            Server.Debug("MSTestRunnerHost.StartRun()");
            SetupUnitTestProvider(_clientTestRunConfiguration.UnitTestProviderType);
            Server.Debug("Completed - SetupUnitTestProvider(" + _clientTestRunConfiguration.UnitTestProviderType + ")");

            var settings = ConfigureSettings();
            Server.Debug("Completed - ConfigureSettings()");

            var ui = UnitTestSystem.CreateTestPage(settings);
            Server.Debug("Completed - UnitTestSystem.CreateTestPage(...)");
            return ui;
        }

        private void CurrentHarness_TestHarnessCompleted(object sender, TestHarnessCompletedEventArgs e)
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
#if MSTestMarch2010
            var settings = new UnitTestSettings();
            settings.TestHarness = new UnitTestHarness();

            DebugOutputProvider item = new DebugOutputProvider();
            item.ShowAllFailures = true;
            settings.LogProviders.Add(item);
            try
            {
                VisualStudioLogProvider visualStudioLogProvider = new VisualStudioLogProvider();
                settings.LogProviders.Add(visualStudioLogProvider);
            }
            catch
            {
            }


            settings.StartRunImmediately = true;
            settings.ShowTagExpressionEditor = false;
            settings.TestService = null;
#else
            var settings = UnitTestSystem.CreateDefaultSettings();
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
