using System;
using System.Net;
using System.Windows;
using StatLight.Client.Harness.Hosts;
using StatLight.Client.Harness.Hosts.MSTest;
using StatLight.Core.Serialization;
using StatLight.Core.WebServer;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness
{
    public partial class App : Application
    {
        private ITestRunnerHost _testRunnerHost = new MSTestRunnerHost();
        private bool _testRunConfigurationDownloadComplete;
        private bool _completedTestXapRequest;

        public App()
        {
            Startup += Application_Startup;
            Exit += Application_Exit;
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
        }

        private void Application_Startup(object sender, StartupEventArgs e)
        {
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

                _testRunnerHost.ConfigureWithClientTestRunConfiguration(clientTestRunConfiguration);

                DisplayTestHarness();
            };
            client.OpenReadAsync(StatLightServiceRestApi.GetTestRunConfiguration.ToFullUri());
        }

        private void Application_Exit(object sender, EventArgs e)
        {
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            try
            {
                LogException(e.ExceptionObject);
            }
            catch (Exception)
            {
            }
            finally
            {
                e.Handled = true;
            }
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

                _testRunnerHost.ConfigureWithLoadedXapData(loadedXapData);

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
                RootVisual = _testRunnerHost.StartRun();
            }
        }

        #endregion

        private static void LogException(Exception ex)
        {
            try
            {
                var msg = ex.ToString();
#if DEBUG
                MessageBox.Show(msg);
#endif

                var messageObject = new UnhandledExceptionClientEvent
                {
                    Exception = ex,
                };
                Server.PostMessage(messageObject);
            }
            catch (Exception)
            {
#if DEBUG
                MessageBox.Show(ex.ToString());
#endif
            }
        }
    }
}
