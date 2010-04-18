using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using StatLight.Core.WebServer;
using UnitDriven;
using StatLight.Client.Harness.Events;
using System.Threading;
using System.Collections.Generic;

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
            Server.Debug("UnitDriven: EntryPointAssembly = {0}".FormatWith(_loadedXapData.EntryPointAssembly));
            var testEngine = new TestEngine(_loadedXapData.EntryPointAssembly);

            WireUpTestEngineMonitoring(testEngine.Context);

            testEngine.Context.Run();

            return testEngine;
        }

        private void WireUpTestEngineMonitoring(TestContext testContext)
        {
            var allMethodTesters = testContext.Testers.SelectMany(s => s.Methods.Select(methods => methods));
            _totalTestsExpectedToRun = allMethodTesters.Count();


            foreach (var methodTester in allMethodTesters)
            {
                methodTester.PropertyChanged += (sender, e) =>
                {
                    var mt = (MethodTester)sender;
                    if (e.PropertyName == "IsRunning" && !mt.IsRunning)
                    {
                        if (mt.Status == TestResult.Fail ||
                            mt.Status == TestResult.Success)
                        {
                            string m = mt.Method.DeclaringType.Namespace +
                                       mt.Method.DeclaringType.ReadClassName() +
                                       mt.Method.Name;
                            lock (_sync)
                            {
                                if (_alreadySentMessages.Contains(m))
                                {
                                    return;
                                }
                            }
                            _alreadySentMessages.Add(m);
                            Server.Trace("%%% TEST msg... %%%");
                            Server.PostMessage(new TestExecutionMethodBeginClientEvent
                                                   {
                                                       NamespaceName = mt.Method.DeclaringType.Namespace,
                                                       ClassName = mt.Method.DeclaringType.ReadClassName(),
                                                       MethodName = mt.Method.Name
                                                   });
                            Interlocked.Increment(ref _totalTestsRun);
                            if (mt.Status == TestResult.Fail)
                            {
                                Server.PostMessage(new TestExecutionMethodFailedClientEvent()
                                {
                                    NamespaceName = mt.Method.DeclaringType.Namespace,
                                    ClassName = mt.Method.DeclaringType.ReadClassName(),
                                    MethodName = mt.Method.Name,
                                    ExceptionInfo = new Exception(mt.Message),
                                });
                            }
                            else if (mt.Status == TestResult.Success)
                            {
                                Server.PostMessage(new TestExecutionMethodPassedClientEvent()
                                {
                                    NamespaceName = mt.Method.DeclaringType.Namespace,
                                    ClassName = mt.Method.DeclaringType.ReadClassName(),
                                    MethodName = mt.Method.Name,
                                });
                            }
                        }
                        if (_totalTestsRun == _totalTestsExpectedToRun)
                            Server.SignalTestComplete();
                    }
                    else
                    {
                        Server.Debug("Status: {0}".FormatWith(mt.Status));
                    }
                };
            }


            testContext.PropertyChanged += (sender, e) =>
            {
                if (testContext.IsRunning)
                {
                    //Server.Debug("UnitDriven: TestContext.PropertyChanged - IsRunning = {0}".FormatWith(testContext.IsRunning));
                }
            };
        }

        private int _totalTestsExpectedToRun = 0;
        private int _totalTestsRun = 0;
        private object _sync = new object();
        private List<string> _alreadySentMessages = new List<string>();
    }
}
