using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Resources;
using System.Xml.Linq;
using StatLight.Client.Harness.Messaging;

namespace StatLight.Client.Harness
{
    public interface ILoadedXapData
    {
        IEnumerable<Assembly> TestAssemblies { get; }
        Assembly EntryPointAssembly { get; }
    }

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

    public class LoadedXapData : LoadedXapDataBase, ILoadedXapData
    {
        public IEnumerable<Assembly> TestAssemblies
        {
            get
            {
                return _testAssemblies.Select(s => s.Value);
            }
        }

        public Assembly EntryPointAssembly { get; private set; }

        public LoadedXapData(Stream xapStream)
        {
            if (xapStream == null)
                throw new ArgumentNullException("xapStream");

            var streamResourceInfo = Application.GetResourceStream(
                new StreamResourceInfo(xapStream, null),
                new Uri("AppManifest.xaml", UriKind.Relative));

            if (streamResourceInfo == null)
                throw new Exception("streamResourceInfo is null");

            string appManifestString = new StreamReader(streamResourceInfo.Stream).ReadToEnd();
            if (appManifestString == null)
                throw new Exception("appManifestString is null");

            XDocument document = XDocument.Parse(appManifestString);
            XElement root = document.Root;
            if (root != null)
            {
                string entryPoint = root.Attribute("EntryPointAssembly").Value;

                //TODO: There has to be a better way to get the Deployment.Parts out of the xml than this...
                var partsElement = root.Elements()
                    .Where(w => w.Name.LocalName.Equals("Deployment.Parts", StringComparison.OrdinalIgnoreCase))
                    .SingleOrDefault();

                if (partsElement != null)
                {
                    var parts = partsElement.Elements()
                        .Select(p => p.Attribute("Source").Value).ToList();
                    Server.Debug("Parts Count = {0}".FormatWith(parts.Count));
                    foreach (var part in parts)
                    {
                        var assemblyPart = new AssemblyPart { Source = part };

                        StreamResourceInfo assemblyStream = Application.GetResourceStream(
                            new StreamResourceInfo(xapStream, "application/binary"),
                            new Uri(assemblyPart.Source, UriKind.Relative));

                        if (assemblyStream == null)
                            throw new Exception(string.Format("Assembly resource missing for [{0}]. (file not found in xap)", assemblyPart.Source));

                        Assembly ass = assemblyPart.Load(assemblyStream.Stream);

                        if (part == entryPoint + ".dll")
                        {
                            EntryPointAssembly = ass;
                        }

                        if (ass != null)
                        {
                            if (ShouldNotIgnoreAssembly(ass))
                            {
                                Server.Debug(ass.FullName);
                                AddAssembly(ass);
                            }
                        }
                    }
                }
                else
                    throw new InvalidOperationException("The application manifest does not contain a Deployment.Parts xml element.");

                if (_testAssemblies.Count == 0)
                    throw new InvalidOperationException("Could not find the entry poing assembly [{0}].".FormatWith(entryPoint));
            }
            else
                throw new InvalidOperationException("The AppManifest's document root was null.");
        }
    }

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