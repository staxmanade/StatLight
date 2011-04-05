using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapInspection
{
    public class SilverlightAssemblyResolver : AssemblyResolverBase
    {
        private readonly Lazy<string> _silverlightToolkitToolFolder;
        private readonly Lazy<string> _silverlightFolder;

        public SilverlightAssemblyResolver(ILogger logger, DirectoryInfo assemblyDirectoryInfo)
            : base(logger, assemblyDirectoryInfo)
        {
            _silverlightFolder = new Lazy<string>(GetSilverlightFolder);
            _silverlightToolkitToolFolder = new Lazy<string>(GetSilverlightToolkitToolFolder);
        }


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


        internal static string GetSilverlightToolkitToolFolder()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\SilverlightToolkit\Tools\v4.0");

            if (registryKey == null)
                throw new StatLightException(@"Could not open the registry and find key hklm\SOFTWARE\Microsoft\Silverlight");

            var silverlightVersion = registryKey.GetValue("") as string;

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

            throw new FileNotFoundException("Could not find assembly [{0}]. The following paths were searched:{1}{2}{1}Try setting the assembly to 'Copy Local=True' in your project so StatLight can attempt to find the assembly.".FormatWith(assemblyName.FullName,
                                                                                                                                                                                                                                                Environment.NewLine, string.Join(Environment.NewLine, _pathsTriedAndFailed.ToArray())));
        }
    }
}