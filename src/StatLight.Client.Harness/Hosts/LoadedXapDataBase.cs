using System.Collections.Generic;
using System.Reflection;
using StatLight.Core.Events.Messaging;

namespace StatLight.Client.Harness.Hosts
{
    public class LoadedXapDataBase
    {
        protected readonly Dictionary<string, Assembly> _testAssemblies = new Dictionary<string, Assembly>();
        protected static bool ShouldNotIgnoreAssembly(Assembly ass)
        {
            return !ass.FullName.StartsWith("Microsoft.Silverlight.Testing,");
        }

        protected void AddAssembly(Assembly ass)
        {
            if (!_testAssemblies.ContainsKey(ass.FullName))
            {
                _testAssemblies.Add(ass.FullName, ass);
            }
        }
    }
}