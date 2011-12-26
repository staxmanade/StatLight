using System.Collections.Generic;
using System.Reflection;

namespace StatLight.Core.Events.Hosts
{
    public interface ILoadedXapData
    {
        IEnumerable<Assembly> TestAssemblies { get; }
        Assembly EntryPointAssembly { get; }
    }
}