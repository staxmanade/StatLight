using System;
using System.ComponentModel.Composition;
using System.Windows;
using StatLight.Core.WebServer;

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
            //return UnitTestSystem.CreateTestPage(settings);
        }

        //TODO: TestComplete event?
        //private static void CurrentHarness_TestHarnessCompleted(object sender, TestHarnessCompletedEventArgs e)
        //{
        //    var state = e.State;
        //    var signalTestCompleteClientEvent = new SignalTestCompleteClientEvent
        //    {
        //        Failed = state.Failed,
        //        TotalFailureCount = state.Failures,
        //        TotalTestsCount = state.TotalScenarios,
        //    };
        //    Server.SignalTestComplete(signalTestCompleteClientEvent);
        //}
    }
}
