using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using StatLight.Core.Configuration;
using StatLight.Client.Harness.Hosts;

namespace StatLight.Client.Harness.Hosts
{
    public interface ITestRunnerHost
    {
        void ConfigureWithClientTestRunConfiguration(ClientTestRunConfiguration clientTestRunConfiguration);
        void ConfigureWithLoadedXapData(ILoadedXapData loadedXapData);
        UIElement StartRun();
    }

    public interface IRemotelyHostedTestRunnerHost
    {
        UIElement StartRun(IEnumerable<Assembly> assemblyToTest);
    }

}