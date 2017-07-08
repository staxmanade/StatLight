using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace StatLight.Core.WebServer.AssemblyResolution
{
    public abstract class AssemblyResolverBase : MarshalByRefObject
    {

        private readonly List<string> _pathsTriedAndFailed = new List<string>();
        protected abstract string ResolveAssemblyPath(AssemblyName assemblyName);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:Identifiers should be spelled correctly")]
        protected string OriginalAssemblyDir { get; private set; }

        public IEnumerable<string> ResolveAllDependentAssemblies(string path)
        {
            OriginalAssemblyDir = new FileInfo(path).DirectoryName;
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

        protected static string ProgramFilesFolder
        {
            get { return Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86); }
        }

        protected bool TryPath(string path)
        {
            if (File.Exists(path))
                return true;

            _pathsTriedAndFailed.Add(path);
            return false;
        }

        protected void ThrowFileNotFound(string fullName)
        {
            throw new FileNotFoundException("Could not find assembly [{0}]. The following paths were searched:{1}{2}{1}Try setting the assembly to 'Copy Local=True' in your project so StatLight can attempt to find the assembly."
                                                .FormatWith(
                                                    fullName,
                                                    Environment.NewLine,
                                                    string.Join(Environment.NewLine, _pathsTriedAndFailed.ToArray())));
        }

    }
}
