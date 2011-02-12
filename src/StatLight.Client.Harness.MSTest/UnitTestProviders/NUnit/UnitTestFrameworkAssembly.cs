using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Silverlight.Testing.Harness;
#if March2010 || April2010 || May2010
#elif Feb2011
#elif May2010 || July2009 || October2009 || November2009
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit
{
    /// <summary>
    /// Assembly metadata for the Visual Studio Team Test unit test framework.
    /// </summary>
    public class UnitTestFrameworkAssembly : UnitTestFrameworkAssemblyBase
    {

        /// <summary>
        /// Creates a new unit test assembly wrapper.
        /// </summary>
        /// <param name="provider">Unit test metadata provider.</param>
        /// <param name="unitTestHarness">A reference to the unit test harness.</param>
        /// <param name="assembly">Assembly reflection object.</param>
        public UnitTestFrameworkAssembly(IUnitTestProvider provider, object unitTestHarness, Assembly assembly)
            :base(provider,unitTestHarness, assembly)
        {
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
            get { return null; }
        }

        /// <summary>
        /// Gets any assembly cleanup method.
        /// </summary>
        public override MethodInfo AssemblyCleanupMethod
        {
            get { return null; }
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
                        type => type.HasAttribute(NUnitAttributes.TestFixture),
                        type => new TestClass(this, type));
        }
    }
}