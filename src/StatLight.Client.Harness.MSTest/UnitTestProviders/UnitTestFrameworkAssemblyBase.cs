using System.Collections.Generic;
using System.Reflection;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
#if March2010 || April2010 || May2010
using ITestHarness = Microsoft.Silverlight.Testing.Harness.UnitTestHarness;
#elif Feb2011

#elif May2010 || July2009 || October2009 || November2009
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#endif


namespace StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders
{
    public abstract class UnitTestFrameworkAssemblyBase : IAssembly
    {
        protected UnitTestFrameworkAssemblyBase(IUnitTestProvider provider, object unitTestHarness, Assembly assembly)
        {
            Provider = provider;
#if Feb2011 || WINDOWS_PHONE
            TestHarness = unitTestHarness as UnitTestHarness;
#else
            TestHarness = unitTestHarness as ITestHarness;
#endif
            Assembly = assembly;
        }

        public abstract ICollection<ITestClass> GetTestClasses();
        public abstract MethodInfo AssemblyInitializeMethod { get; }
        public abstract MethodInfo AssemblyCleanupMethod { get; }
        public IUnitTestProvider Provider { get; private set; }
        public abstract string Name { get; }

        protected Assembly Assembly { get; set; }


#if Feb2011 || WINDOWS_PHONE
        public UnitTestHarness TestHarness { get; set; }
#else
        public ITestHarness TestHarness { get; set; }
#endif

        /// <summary>
        /// Gets the test harness as a unit test harness.
        /// </summary>
        public UnitTestHarness UnitTestHarness
        {
            get { return TestHarness as UnitTestHarness; }
        }
    }
}