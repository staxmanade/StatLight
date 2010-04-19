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
        //private ClientTestRunConfiguration _clientTestRunConfiguration;
        private LoadedXapData _loadedXapData;
        private Dispatcher _dispatcher;
        private int _totalFailedCount;
        private int _totalTestsExpectedToRun;
        private int _totalTestsRun;
        private readonly object _sync = new object();
        private readonly List<string> _alreadySentMessages = new List<string>();

        public void ConfigureWithClientTestRunConfiguration(ClientTestRunConfiguration clientTestRunConfiguration)
        {
            //_clientTestRunConfiguration = clientTestRunConfiguration;
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
            List<MethodTester> allMethodTesters = GetMethodTestersToRun(testContext).ToList();

            _totalTestsExpectedToRun = allMethodTesters.Count;

            foreach (var methodTester in allMethodTesters)
            {
                SetupMethodTesterCommand(methodTester);
            }
        }

        private static IEnumerable<MethodTester> GetMethodTestersToRun(TestContext testContext)
        {
            return testContext.Testers
                .SelectMany(s => s.Methods.Select(methods => methods))
                .Where(w => ClientTestRunConfiguration.ContainsMethod(w.Method));
        }

        private void OnMethodTesterPropertyChanged(MethodTester mt, string propertyName)
        {
            if (propertyName == "IsRunning" && !mt.IsRunning)
            {
                if (mt.Status == TestResult.Fail ||
                    mt.Status == TestResult.Success)
                {
                    var m = mt.Method.FullName();

                    if (!ShouldReportedThis(m))
                        return;

                    // The Server requires both a begin and end test event
                    // It doesn't look easy to report the start of a test thorugh UnitDriven (at least yet...)
                    // So just send it now - an repor the final event right after that.
                    SendTestBeginClientEvent(mt);

                    if (mt.Status == TestResult.Fail)
                    {
                        Interlocked.Increment(ref _totalFailedCount);

                        SendTestFailureClientEvent(mt.Method, mt.Message);
                    }
                    else if (mt.Status == TestResult.Success)
                    {
                        SendTestPassedClientEvent(mt.Method);
                    }

                    Interlocked.Increment(ref _totalTestsRun);
                }

                if (_totalTestsRun == _totalTestsExpectedToRun)
                    SendTestCompleteClientEvent();
            }
            else
            {
                Server.Debug("Status: {0}".FormatWith(mt.Status));
            }
        }

        private bool ShouldReportedThis(string m)
        {
            // Using the property changed events to report messages 
            // puts in a place where we could potentially report multiple 
            // of the same messages... Keep track of what we're reporting 
            // and only report it once.

            lock (_sync)
            {
                if (_alreadySentMessages.Contains(m))
                {
                    return false;
                }

                _alreadySentMessages.Add(m);

                return true;
            }
        }

        private static void SendTestBeginClientEvent(MethodTester methodTester)
        {
            var e = PopulateCoreInfo(new TestExecutionMethodBeginClientEvent(), methodTester.Method);

            Server.PostMessage(e);
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
            methodTester.PropertyChanged += (sender, e) => OnMethodTesterPropertyChanged((MethodTester)sender, e.PropertyName);

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
    }
}
