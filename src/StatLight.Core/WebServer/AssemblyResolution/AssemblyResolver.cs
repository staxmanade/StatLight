using System;
using System.Collections.Generic;

namespace StatLight.Core.WebServer.AssemblyResolution
{
    public class AssemblyResolver
    {

        public IEnumerable<string> ResolveAllDependentAssemblies(bool isPhoneRun, string fullName)
        {
            AppDomain tempDomain = AppDomain.CreateDomain("TemporaryAppDomain");

            AssemblyResolverBase resolver = CreateResolver(isPhoneRun, tempDomain);

            var resolvedDependencies = resolver.ResolveAllDependentAssemblies(fullName);

            AppDomain.Unload(tempDomain);

            return resolvedDependencies;
        }

        private AssemblyResolverBase CreateResolver(bool isPhoneRun, AppDomain tempDomain)
        {
            AssemblyResolverBase resolver;
            if (isPhoneRun)
                resolver = CreateInOtherAppDomain<PhoneAssemblyResolver>(tempDomain);
            else
                resolver = CreateInOtherAppDomain<SilverlightAssemblyResolver>(tempDomain);
            return resolver;
        }

        private AssemblyResolverBase CreateInOtherAppDomain<T>(AppDomain appDomain)
            where T : AssemblyResolverBase
        {
            return (AssemblyResolverBase)appDomain.CreateInstanceAndUnwrap(
                GetType().Assembly.FullName,
                typeof(T).FullName);
        }
    }


}