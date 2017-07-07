using System.Collections.Generic;

namespace StatLight.Core.WebServer.AssemblyResolution
{
    using System;
    using System.IO;
    using System.Reflection;
    using Microsoft.Win32;
    using StatLight.Core.Common;

    //public class AssemblyResolver
    //{
    //    private readonly ILogger _logger;

    //    public AssemblyResolver(ILogger logger)
    //    {
    //        if (logger == null) throw new ArgumentNullException("logger");
    //        _logger = logger;
    //    }

    //    public IEnumerable<string> ResolveAllDependentAssemblies(string path)
    //    {
    //        _logger.Debug("AssemblyResolver - path: {0}".FormatWith(path));

    //        _logger.Debug("Creating new AppDomain to reflect over assembly - " + path);
    //        AppDomain tempDomain = AppDomain.CreateDomain("TemporaryAppDomain");

    //        var instanceAndUnwrap = (AppDomainReflectionManager)tempDomain.CreateInstanceFromAndUnwrap(
    //            GetType().Assembly.CodeBase,
    //            typeof(AppDomainReflectionManager).FullName);

    //        return instanceAndUnwrap.GetAllReferences(path);
    //    }
    //}

    public class SilverlightAssemblyResolver : AssemblyResolverBase
    {
        private readonly Lazy<string> _silverlightToolkitToolFolder;
        private readonly Lazy<string> _silverlightFolder;

        public SilverlightAssemblyResolver()
        {
            _silverlightFolder = new Lazy<string>(GetSilverlightFolder);
            _silverlightToolkitToolFolder = new Lazy<string>(GetSilverlightToolkitToolFolder);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "hklm")]
        internal static string GetSilverlightFolder()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Silverlight");

            if (registryKey == null)
                throw new StatLightException(@"Could not open the registry and find key hklm\SOFTWARE\Microsoft\Silverlight");

            var silverlightVersion = registryKey.GetValue("Version") as string;

            if (silverlightVersion == null)
                throw new StatLightException("Cannot determine the Silverlight version as the registry key lookup returned nothing");

            string silverlightFolder = Path.Combine(ProgramFilesFolder, "Microsoft Silverlight", silverlightVersion);
            if (!Directory.Exists(silverlightFolder))
            {
                throw new DirectoryNotFoundException("Could not find directory " + silverlightFolder);
            }
            return silverlightFolder;
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "hklm")]
        internal static string GetSilverlightToolkitToolFolder()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Silverlight");

            if (registryKey == null)
                throw new StatLightException(@"Could not open the registry and find key hklm\SOFTWARE\Microsoft\Silverlight");

            var silverlightVersion = registryKey.GetValue("Version") as string;

            if (silverlightVersion == null)
                throw new StatLightException("Cannot determine the Silverlight version as the registry key lookup returned nothing");

            string silverlightFolder = Path.Combine(ProgramFilesFolder, "Microsoft Silverlight", silverlightVersion);
            if (!Directory.Exists(silverlightFolder))
            {
                throw new DirectoryNotFoundException("Could not find directory " + silverlightFolder);
            }
            return silverlightFolder;
        }


        protected override string ResolveAssemblyPath(AssemblyName assemblyName)
        {
            if (assemblyName == null) throw new ArgumentNullException("assemblyName");

            if (TryPath(assemblyName.CodeBase))
                return assemblyName.CodeBase;

            string newTestPath = Path.Combine(OriginalAssemblyDir, assemblyName.Name + ".dll");
            if (TryPath(newTestPath))
                return newTestPath;

            newTestPath = Path.Combine(_silverlightFolder.Value, assemblyName.Name + ".dll");
            if (TryPath(newTestPath))
                return newTestPath;

            newTestPath = Path.Combine(_silverlightToolkitToolFolder.Value, assemblyName.Name + ".dll");
            if (TryPath(newTestPath))
                return newTestPath;

            // TODO: Look into how to support the following paths...
            // C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\System.Windows.dll
            // C:\Program Files (x86)\Microsoft SDKs\Silverlight\v4.0

            ThrowFileNotFound(assemblyName.FullName);

            // Should not get here because the above throws.
            throw new NotImplementedException();
        }
    }
}