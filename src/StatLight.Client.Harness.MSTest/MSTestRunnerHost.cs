using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Client.Harness.Events;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.MSTest;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.UnitDriven;
using StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.Xunit;
using StatLight.Client.Harness.Messaging;
using StatLight.Core.Common;
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

        private void SetupUnitTestProvider(UnitTestProviderType unitTestProviderType)
        {
            Microsoft.Silverlight.Testing.UnitTesting.Metadata.UnitTestProviders.Providers.Clear();

            if (unitTestProviderType == UnitTestProviderType.XUnitLight)
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
            else if (unitTestProviderType == UnitTestProviderType.MSTestWithCustomProvider)
            {
                Type interfaceLookingFor = typeof(IUnitTestProvider);

                var allProviderPossibilities = (from assembly in _loadedXapData.TestAssemblies
                                                from type in assembly.GetTypes()
                                                where interfaceLookingFor.IsAssignableFrom(type)
                                                    && !type.IsAbstract
                                                    && type != typeof(Microsoft.Silverlight.Testing.UnitTesting.Metadata.VisualStudio.VsttProvider)
                                                select type).ToList();

                if (allProviderPossibilities.Count == 1)
                {
                    var customProviderType = allProviderPossibilities.First();
                    var instance = Activator.CreateInstance(customProviderType);
                    var provider = (IUnitTestProvider)instance;
                    UnitTestSystem.RegisterUnitTestProvider(provider);
                }
                else
                {
                    if (allProviderPossibilities.Any())
                    {
                        var providers = string.Join(Environment.NewLine, allProviderPossibilities.Select(x => x.FullName).ToArray());
                        // TODO: how to handle this???
                        throw new StatLightException("Multiple unit test provider types where present in the xap, but only one was expected. The types found were: " + Environment.NewLine + providers);
                    }


                    throw new StatLightException("Could not find any classes that inherit from IUnitTestProvider.");
                }
            }
            else
            {
                UnitTestSystem.RegisterUnitTestProvider(new VsttProvider());
            }
        }

        private UnitTestSettings ConfigureSettings()
        {
#if MSTest2009July || MSTest2009October || MSTest2009November
            var settings = UnitTestSystem.CreateDefaultSettings();
#else
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

            // Don't enable a U.I. when not specifying the U.I. Mode.
            if (!_clientTestRunConfiguration.ShowTestingBrowserHost)
            {
                var statLightTestPage = new StatLightTestPage();
                settings.TestHarness.TestPage = statLightTestPage;

                settings.TestPanelType = typeof(StatLightTestPage);
            }

            settings.StartRunImmediately = true;
            settings.ShowTagExpressionEditor = false;
            settings.TestService = null;

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
