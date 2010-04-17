using System;
using System.ComponentModel.Composition;
using System.Windows;
using StatLight.Core.WebServer;
using UnitDriven;

namespace StatLight.Client.Harness.Hosts.UnitDriven
{
    [Export(typeof(ITestRunnerHost))]
    public class UnitDrivenRunnerHost : ITestRunnerHost
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
            throw new NotImplementedException();
            var testEngine = new TestEngine(_loadedXapData.EntryPointAssembly);
            testEngine.Context.Run();

            return testEngine;
        }
    }
}
