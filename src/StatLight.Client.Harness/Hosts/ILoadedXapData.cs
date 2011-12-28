using System.Collections.Generic;
using System.Reflection;

namespace StatLight.Client.Harness.Hosts
{
    public interface ILoadedXapData
    {
        IEnumerable<Assembly> TestAssemblies { get; }
        Assembly EntryPointAssembly { get; }
    }
}