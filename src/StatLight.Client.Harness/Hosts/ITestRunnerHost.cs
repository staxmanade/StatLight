using System.Windows;
using StatLight.Core.WebServer;

namespace StatLight.Client.Harness.Hosts
{
    internal interface ITestRunnerHost
    {
        void ConfigureWithClientTestRunConfiguration(ClientTestRunConfiguration clientTestRunConfiguration);
        void ConfigureWithLoadedXapData(LoadedXapData loadedXapData);
        UIElement StartRun();
    }
}