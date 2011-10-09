using System;
using UnitDriven;
#if MSTEST
//using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NUNIT
//using NUnit.Framework;
//using TestClass = NUnit.Framework.TestFixtureAttribute;
//using TestMethod = NUnit.Framework.TestAttribute;
#endif

namespace StatLight.IntegrationTests.Silverlight.UnitDriven
{
	[TestClass]
	public class ExampleTests
	{
        // UnitDriven doesn't currently support Nested test classes...
        //[TestClass]
        //public class UnitDrivenNestedClassTests
        //{
        //    [TestMethod]
        //    public void this_should_be_a_passing_test()
        //    {
        //        Assert.IsTrue(true);
        //    }
        //}

		[TestMethod]
		public void EmptyTest()
		{
		}

		[TestMethod]
		public void SimpleAssert()
		{
			int expected = 0;
			int actual = 0;

			Assert.AreEqual(expected, actual);
		}

		[TestMethod]
		[ExpectedException(typeof(InvalidOperationException))]
		public void ExpectedException()
		{
			// This is an illustration of a test throwing an expected exception.
			throw new InvalidOperationException();
		}

		[TestMethod]
		public void UnexpectedException()
		{
			// This is an illustration of a test throwing an expected exception.
			throw new InvalidOperationException();
		}

		[TestMethod]
		public void FailureExample()
		{
			int expected = 0;
			int actual = 1;

			Assert.AreEqual(expected, actual);
		}

#if !SILVERLIGHT3
        [TestMethod]
        public void ShouldBeSL5()
        {
            Assert.AreEqual('5', System.Windows.Deployment.Current.RuntimeVersion[0]);
        }
#else
        [TestMethod]
        public void ShouldBeSL3()
        {
            Assert.AreEqual('3', System.Windows.Deployment.Current.RuntimeVersion[0]);
        }
#endif
	}
}