using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using StatLight.Core.Events.Messaging;

namespace StatLight.Client.Harness.Hosts
{
    public class ThisXapData : ILoadedXapData
    {
        public ThisXapData(string entryPointAssembly, IEnumerable<string> testAssemblyFormalNames)
        {
            if (entryPointAssembly == null) throw new ArgumentNullException("entryPointAssembly");
            if (testAssemblyFormalNames == null) throw new ArgumentNullException("testAssemblyFormalNames");

            Server.Debug("ThisXapData.Expected EntryPointAssembly - {0}".FormatWith(entryPointAssembly));
            Server.Debug("ThisXapData - looking for test assemblies");
            Server.Debug("testAssemblyFormalNames.Count() =" + testAssemblyFormalNames.Count());

            _testAssemblies = (from name in testAssemblyFormalNames
                               where IsNotSpecialAssembly(name.Substring(0, name.IndexOf(',')))
                               let assembly = Assembly.Load(name)
                               select assembly).ToArray();

            foreach (Assembly t in _testAssemblies)
            {
                if (t.FullName == entryPointAssembly)
                {
                    Server.Debug("ThisXapData.FoundFile (EntryPointAssembly) - {0}".FormatWith(t.FullName));
                    _entryPointAssembly = t;
                }

                Server.Debug("ThisXapData.FoundFile - {0}".FormatWith(t.FullName));
            }
        }

        private static bool IsNotSpecialAssembly(string name)
        {
            if (name.EndsWith(".resources"))
                return false;

            var specialAssemblies = new[]
            {
                "System.Xml.Linq.dll",
                "System.Xml.Serialization.dll",
                "Microsoft.Silverlight.Testing.dll",
                "Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll",
                "StatLight.Client.Harness.MSTest.dll",
                "StatLight.Client.Harness.dll",
            };

            foreach (var specialAssembly in specialAssemblies)
            {
                if (name.Equals(specialAssembly, StringComparison.OrdinalIgnoreCase))
                    return false;
            }

            return true;
        }

        private readonly Assembly[] _testAssemblies;
        private readonly Assembly _entryPointAssembly;

        public IEnumerable<Assembly> TestAssemblies
        {
            get { return _testAssemblies; }
        }

        public Assembly EntryPointAssembly
        {
            get { return _entryPointAssembly; }
        }
    }
}