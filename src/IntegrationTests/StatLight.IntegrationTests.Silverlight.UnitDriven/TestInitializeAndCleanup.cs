using System;
using UnitDriven;

#if MSTEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NUNIT
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif

namespace StatLight.IntegrationTests.Silverlight.UnitDriven
{
	[TestClass]
	public class TestInitializeAndCleanup
	{
		private bool _initialized = false;

		[TestInitialize]
		public void Initialize()
		{
			_initialized = true;
		}

#if SILVERLIGHT
		[TestMethod]
		public void EnsureInitializeAndCleanup()
		{
			Assert.IsTrue(_initialized);
		}

		[TestCleanup]
		public void Cleanup()
		{
			System.Diagnostics.Debug.WriteLine("cleanup called");
		} 
#else
        [TestMethod]
        public void EnsureInitialize()
        {
            Assert.IsTrue(_initialized);
        }

        [TestCleanup]
        public void Cleanup()
        {
            System.Diagnostics.Debug.WriteLine("cleanup called");
        }
#endif
	}
}