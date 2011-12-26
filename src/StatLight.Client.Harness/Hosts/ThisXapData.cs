using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows;
using StatLight.Core.Events.Messaging;

namespace StatLight.Core.Events.Hosts
{
    public class ThisXapData : ILoadedXapData
    {
        public ThisXapData(string entryPointAssembly)
        {
            Server.Debug("ThisXapData.Expected EntryPointAssembly - {0}".FormatWith(entryPointAssembly));
            Server.Debug("ThisXapData - looking for test assemblies");
            _testAssemblies = (from ap in Deployment.Current.Parts
                               where IsNotSpecialAssembly(ap)
                               let stream = Application.GetResourceStream(new Uri(ap.Source, UriKind.Relative))
                               select new AssemblyPart().Load(stream.Stream)).ToArray();

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

        private static bool IsNotSpecialAssembly(AssemblyPart ap)
        {
            if (ap.Source.Equals("System.Windows.Controls.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (ap.Source.Equals("System.Xml.Linq.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (ap.Source.Equals("System.Xml.Serialization.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (ap.Source.Equals("Microsoft.Silverlight.Testing.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (ap.Source.Equals("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (ap.Source.Equals("StatLight.Client.Harness.MSTest.dll", StringComparison.OrdinalIgnoreCase))
                return false;
            if (ap.Source.Equals("StatLight.Client.Harness.dll", StringComparison.OrdinalIgnoreCase))
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