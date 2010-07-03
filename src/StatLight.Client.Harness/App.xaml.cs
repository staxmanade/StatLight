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
    public partial class App : Application
    {

        private bool _testRunConfigurationDownloadComplete;
        private bool _completedTestXapRequest;

        [Import(typeof(ITestRunnerHost))]
        public ITestRunnerHost TestRunnerHost { get; set; }

        public App()
        {
            Startup += Application_Startup;
            Exit += Application_Exit;
            UnhandledException += Application_UnhandledException;
            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            try
            {
                var compositionContainer = new CompositionContainer(new DeploymentCatalog());
                compositionContainer.SatisfyImportsOnce(this);
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

            if (TestRunnerHost == null)
                Server.Trace("The ITestRunnerHost was not populated by MEF. WTF?");

            Server.Debug("Application_Startup");
            GoGetTheTestRunConfiguration();
            GoGetTheXapUnderTest();
        }

        private void GoGetTheTestRunConfiguration()
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

                TestRunnerHost.ConfigureWithClientTestRunConfiguration(clientTestRunConfiguration);

                DisplayTestHarness();
            };
            client.OpenReadAsync(StatLightServiceRestApi.GetTestRunConfiguration.ToFullUri());
        }

        private void Application_Exit(object sender, EventArgs e)
        {
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            LogException(e.ExceptionObject);
        }

        #region most of the custom StatLight app code
        private void GoGetTheXapUnderTest()
        {
            Server.Debug("GoGetTheXapUnderTest");
            var client = new WebClient
             {
                 AllowReadStreamBuffering = true
             };
            client.OpenReadCompleted += OnXapToTestDownloaded;
            client.OpenReadAsync(StatLightServiceRestApi.GetXapToTest.ToFullUri());
        }

        private void OnXapToTestDownloaded(object sender, OpenReadCompletedEventArgs e)
        {
            Server.Debug("OnXapToTestDownloaded");
            if (e.Error == null)
            {
                var loadedXapData = new LoadedXapData(e.Result);

                TestRunnerHost.ConfigureWithLoadedXapData(loadedXapData);

                _completedTestXapRequest = true;
                DisplayTestHarness();
            }
            else
                LogException(e.Error);
        }

        private void DisplayTestHarness()
        {
            if (_testRunConfigurationDownloadComplete && _completedTestXapRequest)
            {
                RootVisual = TestRunnerHost.StartRun();
            }
        }

        #endregion

        private static void LogException(Exception ex)
        {
            Server.LogException(ex);
        }
    }
}
