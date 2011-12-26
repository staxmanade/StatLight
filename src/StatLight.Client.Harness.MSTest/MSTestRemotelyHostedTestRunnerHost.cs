using System;
using System.Collections.Generic;
using System.Reflection;
using System.Windows;
using Microsoft.Silverlight.Testing;

namespace StatLight.Core.Events.Hosts.MSTest
{
    public class MSTestRemotelyHostedTestRunnerHost : IRemotelyHostedTestRunnerHost
    {
        public UIElement StartRun(IEnumerable<Assembly> assemblyToTest)
        {
            if (assemblyToTest == null)
                throw new ArgumentNullException("assemblyToTest");

            UnitTestSettings unitTestSettings = UnitTestSystem.CreateDefaultSettings();

            foreach (var assembly in assemblyToTest)
            {
                unitTestSettings.TestAssemblies.Add(assembly);
            }

            return UnitTestSystem.CreateTestPage(unitTestSettings);
        }
    }
}