using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using StatLight.Core.WebServer;
using UnitDriven;
using StatLight.Client.Harness.Events;
using System.Threading;
using System.Collections.Generic;
using UnitDriven.Commanding;

namespace StatLight.Client.Harness.Hosts.UnitDriven
{

    [Export(typeof(ITestRunnerHost))]
    public class UnitDrivenRunnerHost : ITestRunnerHost
    {
        private ClientTestRunConfiguration _clientTestRunConfiguration;
        private LoadedXapData _loadedXapData;
        private Dispatcher _dispatcher;

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

            // We're not using UnitDriven't TestEngine to execute the tests
            // it doesn't provide an easy way for us to filter specific tests
            // so we're manually creating test commands.
            // The only purpose for the TestEngine is to do the original reflection to find what tests to run
            // and to provide the UI...

            var testEngine = new TestEngine(_loadedXapData.EntryPointAssembly);

            Application.Current.RootVisual = testEngine;

            _dispatcher = Application.Current.RootVisual.Dispatcher;

            WireUpTestEngineMonitoring(testEngine.Context);

            return testEngine;
        }

        private void WireUpTestEngineMonitoring(TestContext testContext)
        {
            // Get a list of all the test methods & filter out any un-wanted
            // tests based on the ClientTestRunConfiguration
            var allMethodTesters = testContext.Testers
                .SelectMany(s => s.Methods.Select(methods => methods))
                .Where(w => ClientTestRunConfiguration.ContainsMethod(w.Method))
                .ToList();

            _totalTestsExpectedToRun = allMethodTesters.Count();

            foreach (var methodTester in allMethodTesters)
            {
                methodTester.PropertyChanged += (sender, e) =>
                    OnMethodTesterPropertyChanged((MethodTester)sender, e.PropertyName);
            }

            foreach (var methodTester in allMethodTesters)
            {
                SetupMethodTesterCommand(methodTester);
            }
        }

        private void OnMethodTesterPropertyChanged(MethodTester mt, string propertyName)
        {
            if (propertyName == "IsRunning" && !mt.IsRunning)
            {
                if (mt.Status == TestResult.Fail ||
                    mt.Status == TestResult.Success)
                {

                    var m = mt.Method.FullName();
                    lock (_sync)
                    {
                        if (_alreadySentMessages.Contains(m))
                        {
                            return;
                        }
                    }
                    _alreadySentMessages.Add(m);
                    Server.PostMessage(new TestExecutionMethodBeginClientEvent
                                           {
                                               NamespaceName = mt.Method.DeclaringType.Namespace,
                                               ClassName = mt.Method.DeclaringType.ReadClassName(),
                                               MethodName = mt.Method.Name
                                           });
                    Interlocked.Increment(ref _totalTestsRun);
                    if (mt.Status == TestResult.Fail)
                    {
                        Interlocked.Increment(ref _totalFailedCount);

                        SendTestFailureClientEvent(mt.Method, mt.Message);
                    }
                    else if (mt.Status == TestResult.Success)
                    {
                        SendTestPassedClientEvent(mt.Method);
                    }
                }

                if (_totalTestsRun == _totalTestsExpectedToRun)
                    SendTestCompleteClientEvent();
            }
            else
            {
                Server.Debug("Status: {0}".FormatWith(mt.Status));
            }
        }

        private static void SendTestFailureClientEvent(MethodInfo method, string message)
        {
            var failureEvent = new TestExecutionMethodFailedClientEvent
                                   {
                                       ExceptionInfo = new Exception(message),
                                   };

            PopulateCoreInfo(failureEvent, method);

            Server.PostMessage(failureEvent);
        }

        private static void SendTestPassedClientEvent(MethodInfo method)
        {
            var e = PopulateCoreInfo(new TestExecutionMethodPassedClientEvent(), method);

            Server.PostMessage(e);
        }

        private static TestExecutionMethod PopulateCoreInfo(TestExecutionMethod testExecutionMethod, MethodInfo method)
        {
            testExecutionMethod.NamespaceName = method.DeclaringType.Namespace;
            testExecutionMethod.ClassName = method.DeclaringType.ReadClassName();
            testExecutionMethod.MethodName = method.Name;
            return testExecutionMethod;
        }

        private void SendTestCompleteClientEvent()
        {
            Server.SignalTestComplete(
                new SignalTestCompleteClientEvent
                    {
                        Failed = (_totalFailedCount > 0),
                        TotalFailureCount = _totalFailedCount,
                        TotalTestsCount = _totalTestsExpectedToRun,
                    });
        }

        private void SetupMethodTesterCommand(MethodTester methodTester)
        {
            var command = new TestCommand(methodTester);
            command.Complete += (o, e) => OnCommandComplete(methodTester, (TestCompleteEventArgs)e);
            CommandQueue.Enqueue(command);
        }

        private void OnCommandComplete(MethodTester methodTester, TestCompleteEventArgs e)
        {
            _dispatcher.BeginInvoke(() => SetMethodTesterStatus(methodTester, e));
        }

        private static void SetMethodTesterStatus(MethodTester methodTester2, TestCompleteEventArgs args)
        {
            methodTester2.Message = args.Message;
            methodTester2.Status = args.Result;
            methodTester2.IsRunning = false;
        }

        private int _totalFailedCount;
        private int _totalTestsExpectedToRun;
        private int _totalTestsRun;
        private readonly object _sync = new object();
        private readonly List<string> _alreadySentMessages = new List<string>();
    }
}
