using System;
using System.IO;
using System.Reflection;

namespace StatLight.Core.WebServer.AssemblyResolution
{
    public class PhoneAssemblyResolver : AssemblyResolverBase
    {
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

            ThrowFileNotFound(assemblyName.FullName);

            // Should not get here because the above throws.
            throw new NotImplementedException();
        }

    }
}