using System;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Silverlight.Testing.Harness;
#if March2010 || April2010 || May2010
using ITestHarness = Microsoft.Silverlight.Testing.Harness.UnitTestHarness;
#elif Feb2011 || WINDOWS_PHONE
using ITestHarness = Microsoft.Silverlight.Testing.UnitTesting.Metadata;
#else
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.MSTest
{
    /// <summary>
    /// Assembly metadata for the Visual Studio Team Test unit test framework.
    /// </summary>
    public class UnitTestFrameworkAssembly : UnitTestFrameworkAssemblyBase
    {
        /// <summary>
        /// Assembly initialization method information.
        /// </summary>
        private LazyDynamicAssemblyMethodInfo _init;

        /// <summary>
        /// Assembly cleanup method information.
        /// </summary>
        private LazyDynamicAssemblyMethodInfo _cleanup;

        /// <summary>
        /// Creates a new unit test assembly wrapper.
        /// </summary>
        /// <param name="provider">Unit test metadata provider.</param>
        /// <param name="unitTestHarness">A reference to the unit test harness.</param>
        /// <param name="assembly">Assembly reflection object.</param>
        public UnitTestFrameworkAssembly(IUnitTestProvider provider, object unitTestHarness, Assembly assembly)
            :base (provider,unitTestHarness,assembly)
        {
            _init = new LazyDynamicAssemblyMethodInfo(base.Assembly, ProviderAttributes.AssemblyInitialize);
            _cleanup = new LazyDynamicAssemblyMethodInfo(base.Assembly, ProviderAttributes.AssemblyCleanup);
        }

        /// <summary>
        /// Gets the name of the test assembly.
        /// </summary>
        public override string Name
        {
            get
            {
                string n = base.Assembly.ToString();
                return (n.Contains(", ") ? n.Substring(0, n.IndexOf(",", StringComparison.Ordinal)) : n);
            }
        }

        /// <summary>
        /// Gets any assembly initialize method.
        /// </summary>
        public override MethodInfo AssemblyInitializeMethod
        {
            get { return _init.GetMethodInfo(); }
        }

        /// <summary>
        /// Gets any assembly cleanup method.
        /// </summary>
        public override MethodInfo AssemblyCleanupMethod
        {
            get { return _cleanup.GetMethodInfo(); }
        }

        /// <summary>
        /// Reflect and retrieve the test class metadata wrappers for 
        /// the test assembly.
        /// </summary>
        /// <returns>Returns a collection of test class metadata 
        /// interface objects.</returns>
        public override ICollection<ITestClass> GetTestClasses()
        {
            return base.Assembly.GetTestClasses(
                type => type.HasAttribute(ProviderAttributes.TestClass),
                type => new TestClass(this, type));
        }
    }
}