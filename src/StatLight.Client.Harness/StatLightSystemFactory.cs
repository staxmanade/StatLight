using System;
using System.Reflection;
using System.Windows;
using StatLight.Client.Harness.Hosts;

namespace StatLight
{
    public static class StatLightSystemFactory
    {
        public static void Run(Assembly assemblyToTest, Action<UIElement> rootVisualActionOnReady)
        {
            new RemoteRunStatLightSystem(assemblyToTest, rootVisualActionOnReady);
        }
    }
}