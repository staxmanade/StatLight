using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Microsoft.Win32;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapInspection
{
    public class AssemblyResolver
    {
        private readonly ILogger _logger;

        public AssemblyResolver(ILogger logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        public IEnumerable<string> ResolveAllDependentAssemblies(string path)
        {
            _logger.Debug("AssemblyResolver - path: {0}".FormatWith(path));

            _logger.Debug("Creating new AppDomain to reflect over assembly - " + path);
            AppDomain tempDomain = AppDomain.CreateDomain("TemporaryAppDomain");

            var instanceAndUnwrap = (AppDomainReflectionManager)tempDomain.CreateInstanceAndUnwrap(
                GetType().Assembly.FullName,
                typeof (AppDomainReflectionManager).FullName);

            return instanceAndUnwrap.GetAllReferences(path);
        }
    }


    /// <summary>
    /// Handles doing a little reflection over in another 
    /// AppDomain so we don't run into problems when running 
    /// against multiple versions in different paths.
    /// </summary>
    [Serializable]
    public class AppDomainReflectionManager : MarshalByRefObject
    {
        private string _originalAssemblyDir;
        private Lazy<string> _silverlightFolder;

        public IEnumerable<string> GetAllReferences(string path)
        {
            _silverlightFolder = new Lazy<string>(SilverlightFolder);
            _originalAssemblyDir = new FileInfo(path).DirectoryName;
            var assemblies = new List<string>();
            IncludePdb(assemblies, path);

            Assembly asm = LoadAssembly(path);

            foreach (var assembly in asm.GetReferencedAssemblies())
            {
                BuildDependentAssemblyList(assembly, assemblies);
            }

            return assemblies;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "path")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "assemblies")]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        private void IncludePdb(List<string> assemblies, string path)
        {
            /*
             * When including the pdb's it doesn't appear to help - meaning we still don't see line numbers...
             * 
            if (path.Length > 4)
            {
                var pdbFileName = path.Substring(0, path.Length - 4) + ".pdb";

                if (File.Exists(pdbFileName))
                {
                    _logger.Debug("Resolved Assembly's PDB - {0}".FormatWith(pdbFileName));
                    assemblies.Add(pdbFileName);
                }
                else
                {
                    _logger.Debug("Cannot resolve Assembly's PDB - {0}".FormatWith(pdbFileName));
                }

            }
            */
        }


        private void BuildDependentAssemblyList(AssemblyName assemblyName, List<string> assemblies)
        {
            if (assemblies == null) throw new ArgumentNullException("assemblies");

            var path = ResolveAssemblyPath(assemblyName);

            // Don't load assemblies we've already worked on.
            if (assemblies.Contains(path))
            {
                return;
            }
            Assembly asm = LoadAssembly(path);

            if (asm != null)
            {
                assemblies.Add(path);

                IncludePdb(assemblies, path);

                foreach (AssemblyName item in asm.GetReferencedAssemblies())
                {
                    BuildDependentAssemblyList(item, assemblies);
                }
            }

            var temp = new string[assemblies.Count];
            assemblies.CopyTo(temp, 0);
            return;
        }


        private static Assembly LoadAssembly(string path)
        {
            if (path == null) throw new ArgumentNullException("path");
            Assembly asm;

            // Look for common path delimiters in the string to see if it is a name or a path.
            if ((path.IndexOf(Path.DirectorySeparatorChar, 0, path.Length) != -1) ||
                (path.IndexOf(Path.AltDirectorySeparatorChar, 0, path.Length) != -1))
            {
                // Load the assembly from a path.
                asm = Assembly.ReflectionOnlyLoadFrom(path);
            }
            else
            {
                // Try as assembly name.
                asm = Assembly.ReflectionOnlyLoad(path);
            }
            return asm;
        }

        private static string SilverlightFolder()
        {
            RegistryKey registryKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Silverlight");

            if (registryKey == null)
                throw new StatLightException(@"Could not open the registry and find key hklm\SOFTWARE\Microsoft\Silverlight");

            var silverlightVersion = registryKey.GetValue("Version") as string;

            if (silverlightVersion == null)
                throw new StatLightException("Cannot determine the Silverlight version as the registry key lookup returned nothing");

            string programFilesFolder = Getx86ProgramFilesFolder();
            string silverlightFolder = Path.Combine(programFilesFolder, "Microsoft Silverlight", silverlightVersion);
            if (!Directory.Exists(silverlightFolder))
            {
                throw new DirectoryNotFoundException("Could not find directory " + silverlightFolder);
            }
            return silverlightFolder;
        }

        private static string Getx86ProgramFilesFolder()
        {
            // Copied from http://stackoverflow.com/questions/194157/c-how-to-get-program-files-x86-on-vista-x64
            if (8 == IntPtr.Size || (!String.IsNullOrEmpty(Environment.GetEnvironmentVariable("PROCESSOR_ARCHITEW6432"))))
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }
            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        private string ResolveAssemblyPath(AssemblyName assemblyName)
        {
            var pathsTried = new List<string>();
            Func<string, bool> tryPath = path =>
            {
                if (File.Exists(path))
                    return true;

                pathsTried.Add(path);
                //Log path checked and not found
                return false;
            };


            if (tryPath(assemblyName.CodeBase))
                return assemblyName.CodeBase;

            string newTestPath = Path.Combine(_originalAssemblyDir, assemblyName.Name + ".dll");
            if (tryPath(newTestPath))
                return newTestPath;

            newTestPath = Path.Combine(_silverlightFolder.Value, assemblyName.Name + ".dll");
            if (tryPath(newTestPath))
                return newTestPath;

            // TODO: Look into how to support the following paths...
            // C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\System.Windows.dll
            // C:\Program Files (x86)\Microsoft SDKs\Silverlight\v4.0

            throw new FileNotFoundException("Could not find assembly [{0}]. The following paths were searched:{1}{2}{1}Try setting the assembly to 'Copy Local=True' in your project so StatLight can attempt to find the assembly.".FormatWith(assemblyName.FullName,
                                                                                                                                 Environment.NewLine, string.Join(Environment.NewLine, pathsTried.ToArray())));
        }
    }

}