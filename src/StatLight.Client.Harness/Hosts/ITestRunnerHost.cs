using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using StatLight.Core.Configuration;

namespace StatLight.Core.Events.Hosts
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