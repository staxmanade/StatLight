using System;
using System.IO;
using System.Reflection;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapInspection
{
    public class PhoneAssemblyResolver : AssemblyResolverBase
    {
        public PhoneAssemblyResolver(ILogger logger, FileSystemInfo assemblyDirectoryInfo)
            : base(logger, assemblyDirectoryInfo)
        {
        }

        protected override string ResolveAssemblyPath(AssemblyName assemblyName)
        {
            if (assemblyName == null) throw new ArgumentNullException("assemblyName");
            string pf = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

            string newTestPath = Path.Combine(OriginalAssemblyDir, assemblyName.Name + ".dll");
            if (TryPath(newTestPath))
                return newTestPath;

            if (TryPath(assemblyName.CodeBase))
                return assemblyName.CodeBase;

            //C:\Program Files (x86)\Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\Profile\WindowsPhone\Microsoft.Phone.dll

            newTestPath = Path.Combine(pf, @"Reference Assemblies\Microsoft\Framework\Silverlight\v4.0\Profile\WindowsPhone\", assemblyName.Name + ".dll");
            if (TryPath(newTestPath))
                return newTestPath;

            throw new FileNotFoundException("Could not find assembly [{0}]. The following paths were searched:{1}{2}{1}Try setting the assembly to 'Copy Local=True' in your project so StatLight can attempt to find the assembly.".FormatWith(
                assemblyName.FullName,
                Environment.NewLine,
                string.Join(Environment.NewLine, _pathsTriedAndFailed.ToArray())));
        }
    }
}