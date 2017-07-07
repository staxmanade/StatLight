using System;
using System.Windows;
using StatLight.Client.Harness.Hosts;
using StatLight.Core.Events.Messaging;

namespace StatLight.Client.Harness
{
    public partial class App : Application
    {

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
            var statLightSystem = new NormalStatLightSystem(newRootVisual => RootVisual = newRootVisual);
        }

        private void Application_Exit(object sender, EventArgs e)
        {
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Server.LogException(e.ExceptionObject);
        }
    }
}
