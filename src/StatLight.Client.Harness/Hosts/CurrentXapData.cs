using System.Collections.Generic;
using System.Reflection;

namespace StatLight.Client.Harness.Hosts
{
    public class CurrentXapData : LoadedXapDataBase, ILoadedXapData
    {
        private new readonly IEnumerable<Assembly> _testAssemblies;
        public CurrentXapData(Assembly entryPointAssembly)
        {
            EntryPointAssembly = entryPointAssembly;
            _testAssemblies = new List<Assembly>
                                  {
                                      entryPointAssembly,
                                  };
        }

        public IEnumerable<Assembly> TestAssemblies
        {
            get
            {
                return _testAssemblies;
                //return _testAssemblies.Select(s => s.Value);
            }
        }

        public Assembly EntryPointAssembly { get; private set; }
    }
}