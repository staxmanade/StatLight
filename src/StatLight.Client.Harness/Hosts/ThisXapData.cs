using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using StatLight.Client.Harness.Messaging;

namespace StatLight.Client.Harness.Hosts
{
    public class ThisXapData : ILoadedXapData
    {
        public ThisXapData(string entryPointAssembly, IEnumerable<string> testAssemblyFormalNames)
        {
            Server.Debug("ThisXapData.Expected EntryPointAssembly - {0}".FormatWith(entryPointAssembly));
            Server.Debug("ThisXapData - looking for test assemblies");
#if WINDOWS_PHONE
            _testAssemblies = (from name in testAssemblyFormalNames
                               where IsNotSpecialAssembly(name.Substring(0, name.IndexOf(',')))
                               let assembly = Assembly.Load(name)
                               select assembly).ToArray();
#else
            _testAssemblies = (from ap in Deployment.Current.Parts
                               where IsNotSpecialAssembly(ap.Source)
                               let stream = Application.GetResourceStream(new Uri(ap.Source, UriKind.Relative))
                               select new AssemblyPart().Load(stream.Stream)).ToArray();
#endif

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
            if (name.Equals("System.Windows.Controls.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (name.Equals("System.Xml.Linq.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (name.Equals("System.Xml.Serialization.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (name.Equals("Microsoft.Silverlight.Testing.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (name.Equals("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (name.Equals("StatLight.Client.Harness.MSTest.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (name.Equals("StatLight.Client.Harness.dll", StringComparison.OrdinalIgnoreCase))
                return false;

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