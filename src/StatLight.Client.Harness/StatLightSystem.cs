using System;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Net;
using System.Reflection;
using System.Windows;
using StatLight.Client.Harness.Hosts;
using StatLight.Core.Configuration;
using StatLight.Core.Serialization;
using StatLight.Core.WebServer;

namespace StatLight.Client.Harness
{
    public class StatLightSystem
    {
        private bool _testRunConfigurationDownloadComplete;
        private bool _completedTestXapRequest;
        private ITestRunnerHost _testRunnerHost;
        private Action<UIElement> _onReady;

        public void OnReadySetupRootVisual(Action<UIElement> onReady)
        {
            if (onReady == null)
                throw new ArgumentNullException("onReady");

            _onReady = onReady;

            _testRunnerHost = DiscoverITestRunnerHost();

            GoGetTheTestRunConfiguration();
        }

        public void GoGetTheTestRunConfiguration()
        {
            var client = new WebClient
            {
                AllowReadStreamBuffering = true
            };

            client.OpenReadCompleted += (sender, e) =>
            {
                var clientTestRunConfiguration = e.Result.Deserialize<ClientTestRunConfiguration>();
                ClientTestRunConfiguration.CurrentClientTestRunConfiguration = clientTestRunConfiguration;
                _testRunConfigurationDownloadComplete = true;

                _testRunnerHost.ConfigureWithClientTestRunConfiguration(clientTestRunConfiguration);
                Server.Debug("XapToTestUrl: {0}".FormatWith(clientTestRunConfiguration.XapToTestUrl));
                GoGetTheXapUnderTest(clientTestRunConfiguration.XapToTestUrl.ToUri());
            };
            client.OpenReadAsync(StatLightServiceRestApi.GetTestRunConfiguration.ToFullUri());

        }

        private void GoGetTheXapUnderTest(Uri xapToTestUri)
        {
            Server.Debug("GoGetTheXapUnderTest");
            var client = new WebClient
            {
                AllowReadStreamBuffering = true
            };
            client.OpenReadCompleted += OnXapToTestDownloaded;
            client.OpenReadAsync(xapToTestUri);
        }

        private void OnXapToTestDownloaded(object sender, OpenReadCompletedEventArgs e)
        {
            Server.Debug("OnXapToTestDownloaded");
            if (e.Error == null)
            {
                var loadedXapData = new LoadedXapData(e.Result);

                _testRunnerHost.ConfigureWithLoadedXapData(loadedXapData);

                _completedTestXapRequest = true;
                DisplayTestHarness();
            }
            else
                Server.LogException(e.Error);
        }

        private void DisplayTestHarness()
        {
            if (_testRunConfigurationDownloadComplete && _completedTestXapRequest)
            {
                var rootVisual = _testRunnerHost.StartRun();
                _onReady(rootVisual);
            }
        }

        private static ITestRunnerHost DiscoverITestRunnerHost()
        {
            ITestRunnerHost testRunnerHost = null;

            try
            {
                var compositionContainer = new CompositionContainer(new DeploymentCatalog());

                testRunnerHost = compositionContainer.GetExportedValue<ITestRunnerHost>();
            }
            catch (ReflectionTypeLoadException rfex)
            {
                ReflectionInfoHelper.HandleReflectionTypeLoadException(rfex);
            }
            catch (CompositionException compositionException)
            {
                Server.Trace(compositionException.ToString());
                foreach (var err in compositionException.Errors)
                    Server.Trace(err.ToString());
            }

            if (testRunnerHost == null)
                Server.Trace("The ITestRunnerHost was not populated by MEF. WTF?");

            return testRunnerHost;
        }
    }
}