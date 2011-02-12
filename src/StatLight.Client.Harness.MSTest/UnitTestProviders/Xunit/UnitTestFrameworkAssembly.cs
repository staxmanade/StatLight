using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.Silverlight.Testing.Harness;
#if March2010 || April2010 || May2010
using ITestHarness = Microsoft.Silverlight.Testing.Harness.UnitTestHarness;
#elif Feb2011
using ITestHarness = Microsoft.Silverlight.Testing.UnitTesting.Metadata;
#else
using Microsoft.Silverlight.Testing.UnitTesting.Harness;
#endif
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.Xunit
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
            :base(provider, unitTestHarness, assembly)
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
            var allTypes = base.Assembly.GetTypes();
            var classes = allTypes.Where(ContainsAMethodWithAFactAttribute).ToList();

            List<ITestClass> tests = new List<ITestClass>(classes.Count);
            foreach (Type type in classes)
            {
                tests.Add(new TestClass(this, type));
            }
            return tests;
        }

        private bool ContainsAMethodWithAFactAttribute(Type type)
        {
            if (type.IsAbstract)
                return false;

            if (type.IsPublic || type.IsNestedPublic)
            {

                if (TestClass.GetTestMethods(type).Count > 0)
                {
                    return true;
                }

                foreach (Type t in type.GetNestedTypes(BindingFlags.Public))
                {
                    if (TestClass.GetTestMethods(t).Count > 0)
                        return true;
                }
            }

            return false;
        }
    }

}