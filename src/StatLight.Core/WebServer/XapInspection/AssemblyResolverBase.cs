using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapInspection
{
    public abstract class AssemblyResolverBase
    {
        private readonly ILogger _logger;
        protected string OriginalAssemblyDir { get; private set; }

        protected AssemblyResolverBase(ILogger logger, FileSystemInfo assemblyDirectoryInfo)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (assemblyDirectoryInfo == null) throw new ArgumentNullException("assemblyDirectoryInfo");

            _logger = logger;
            OriginalAssemblyDir = assemblyDirectoryInfo.FullName;

            _logger.Debug("AssemblyResolver - OriginalAssembly - [{0}]".FormatWith(OriginalAssemblyDir));
        }

        public IEnumerable<string> ResolveAllDependentAssemblies(string path)
        {
            _logger.Debug("AssemblyResolver - path: {0}".FormatWith(path));
            Assembly reflectionOnlyLoadFrom = Assembly.ReflectionOnlyLoadFrom(path);
            Debug.Assert(reflectionOnlyLoadFrom != null);
            AssemblyName[] referencedAssemblies = reflectionOnlyLoadFrom.GetReferencedAssemblies();


            var assemblies = new List<string>();

            foreach (var assembly in referencedAssemblies)
            {
                BuildDependentAssemblyList(assembly, assemblies);
            }

            return assemblies;
        }

        protected abstract string ResolveAssemblyPath(AssemblyName assemblyName);

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

        protected static string ProgramFilesFolder
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86); }
        }

        protected List<string> _pathsTriedAndFailed = new List<string>();

        protected bool TryPath(string path)
        {
            if (File.Exists(path))
                return true;

            _pathsTriedAndFailed.Add(path);
            return false;
        }

    }
}