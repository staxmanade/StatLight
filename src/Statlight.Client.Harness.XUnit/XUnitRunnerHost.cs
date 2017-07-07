using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using StatLight.Client.Harness.Hosts;
using StatLight.Core.Configuration;
using StatLight.Core.Events;
using StatLight.Core.Events.Messaging;
using Xunit.Runner.Silverlight;

namespace StatLight.Client.Harness.XUnit
{
    public class XUnitRunnerHost : ITestRunnerHost
    {
        private ILoadedXapData _loadedXapData;
        private Dispatcher _dispatcher;
        private int _totalFailedCount;
        private int _totalTestsExpectedToRun;
        private int _totalTestsRun;
        private readonly object _sync = new object();
        private readonly List<string> _alreadySentMessages = new List<string>();

        public void ConfigureWithClientTestRunConfiguration(ClientTestRunConfiguration clientTestRunConfiguration)
        {
        }

        public void ConfigureWithLoadedXapData(ILoadedXapData loadedXapData)
        {
            _loadedXapData = loadedXapData;
        }

        public UIElement StartRun()
        {
            Server.Debug("XUnit: EntryPointAssembly = {0}".FormatWith(_loadedXapData.EntryPointAssembly));

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
            _totalTestsExpectedToRun = 0;

            if (testContext.Total == 0)
            {
                SendTestCompleteClientEvent();
                return;
            }

            var methodTesters = GetMethodTestersToRun(testContext);
            _totalTestsExpectedToRun = methodTesters.Count();
            foreach (var methodTester in methodTesters)
            {
                SetupMethodTesterCommand(methodTester);
            }
        }

        private static IEnumerable<MethodTester> GetMethodTestersToRun(TestContext testContext)
        {
            return GetMethodTesters(testContext).Where(w => ClientTestRunConfiguration.ContainsMethod(w.Method.MethodInfo));
        }

        private static IEnumerable<MethodTester> GetMethodTesters(TestGroup group)
        {
            foreach (var item in group.Items)
            {
                var groupItem = item as TestGroup;
                if (groupItem != null)
                {
                    foreach (var subItem in GetMethodTesters(groupItem))
                        yield return subItem;
                    continue;
                }

                var methodTester = item as MethodTester;
                if (methodTester != null)
                {
                    yield return methodTester;
                    continue;
                }

                throw new InvalidOperationException();
            }
        }

        private void OnMethodTesterPropertyChanged(MethodTester mt, string propertyName)
        {
            if (propertyName == "IsRunning" && mt.IsRunning)
            {
                SendTestBeginClientEvent(mt);
            }

            if (propertyName == "IsRunning" && !mt.IsRunning)
            {
                if (IsFinalResultStatus(mt))
                {
                    MethodInfo methodInfo = mt.Method.MethodInfo;
                    if (!ShouldReportedThisInstanceOfTheFinalResult(methodInfo))
                        return;

                    switch (mt.Status)
                    {
                        case TestResult.Skip:
                            SendTestIgnoredClientEvent(methodInfo, mt.Message);
                            break;
                        case TestResult.Fail:
                            SendTestFailureClientEvent(methodInfo, mt.Message);
                            Interlocked.Increment(ref _totalFailedCount);
                            break;
                        case TestResult.Success:
                            SendTestPassedClientEvent(methodInfo);
                            break;
                    }

                    Interlocked.Increment(ref _totalTestsRun);

                    if (_totalTestsRun == _totalTestsExpectedToRun)
                        SendTestCompleteClientEvent();
                }
            }
        }

        private static bool IsFinalResultStatus(MethodTester methodTester)
        {
            return methodTester.Status == TestResult.Fail ||
                   methodTester.Status == TestResult.Success ||
                   methodTester.Status == TestResult.Skip;
        }

        private bool ShouldReportedThisInstanceOfTheFinalResult(MethodInfo methodInfo)
        {
            var fullName = methodInfo.FullName();

            // Using the property changed events to report messages 
            // puts in a place where we could potentially report multiple 
            // of the same messages... Keep track of what we're reporting 
            // and only report it once.
            lock (_sync)
            {
                if (_alreadySentMessages.Contains(fullName))
                {
                    return false;
                }

                _alreadySentMessages.Add(fullName);

                return true;
            }
        }

        private static void SendTestBeginClientEvent(MethodTester methodTester)
        {
            var e = PopulateCoreInfo(new TestExecutionMethodBeginClientEvent(), methodTester.Method.MethodInfo);

            Server.PostMessage(e);
        }

        private static void SendTestFailureClientEvent(MethodInfo method, string message)
        {
            var failureEvent = new TestExecutionMethodFailedClientEvent
                                   {
                                       ExceptionInfo = new Exception(message),
                                       Started = new DateTime(),
                                       Finished = new DateTime(),
                                   };

            PopulateCoreInfo(failureEvent, method);

            Server.PostMessage(failureEvent);
        }

        private static void SendTestIgnoredClientEvent(MethodInfo method, string message)
        {
            var ignoredEvent = new TestExecutionMethodIgnoredClientEvent
                                   {
                                       Message = message,
                                       Started = new DateTime(),
                                       Finished = new DateTime(),
                                   };

            PopulateCoreInfo(ignoredEvent, method);

            Server.PostMessage(ignoredEvent);
        }

        private static void SendTestPassedClientEvent(MethodInfo method)
        {
            var passedClientEvent = new TestExecutionMethodPassedClientEvent
                                        {
                                            Started = new DateTime(),
                                            Finished = new DateTime(),
                                        };
            var e = PopulateCoreInfo(passedClientEvent, method);

            Server.PostMessage(e);
        }

        private static TestExecutionMethodClientEvent PopulateCoreInfo(TestExecutionMethodClientEvent testExecutionMethodClientEvent, MethodInfo method)
        {
            testExecutionMethodClientEvent.NamespaceName = method.ReflectedType.Namespace;
            testExecutionMethodClientEvent.ClassName = method.ReflectedType.ClassNameIncludingParentsIfNested();
            testExecutionMethodClientEvent.MethodName = method.Name;
            return testExecutionMethodClientEvent;
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
            methodTester.RunTests();
        }
    }
}
