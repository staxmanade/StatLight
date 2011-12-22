using System.Collections.Generic;
using System.Reflection;
using Microsoft.Silverlight.Testing.Harness;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
#if MSTest2010March || MSTest2010April || MSTest2010May
using ITestHarness = Microsoft.Silverlight.Testing.Harness.UnitTestHarness;
#elif MSTest2011Feb

#elif MSTest2010May || MSTest2009July || MSTest2009October || MSTest2009November
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#endif


namespace StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders
{
    public abstract class UnitTestFrameworkAssemblyBase : IAssembly
    {
        protected UnitTestFrameworkAssemblyBase(IUnitTestProvider provider, object unitTestHarness, Assembly assembly)
        {
            Provider = provider;
#if MSTest2010March || MSTest2010April || MSTest2010May || MSTest2009July || MSTest2009October || MSTest2009November
            TestHarness = unitTestHarness as ITestHarness;
#else
            TestHarness = unitTestHarness as UnitTestHarness;
#endif
            Assembly = assembly;
        }

        public abstract ICollection<ITestClass> GetTestClasses();
        public abstract MethodInfo AssemblyInitializeMethod { get; }
        public abstract MethodInfo AssemblyCleanupMethod { get; }
        public IUnitTestProvider Provider { get; private set; }
        public abstract string Name { get; }

        protected Assembly Assembly { get; set; }

#if MSTest2010March || MSTest2010April || MSTest2010May || MSTest2009July || MSTest2009October || MSTest2009November
        public ITestHarness TestHarness { get; set; }
#else
        public UnitTestHarness TestHarness { get; set; }
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
