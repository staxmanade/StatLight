using System;
using System.ComponentModel;
using UnitDriven;
using System.Threading;

#if MSTEST
using Microsoft.VisualStudio.TestTools.UnitTesting;
#elif NUNIT
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
#endif

namespace StatLight.IntegrationTests.Silverlight.UnitDriven
{
	[TestClass]
	public class AsyncTests : TestBase
	{
		[TestMethod]
		public void Simple1()
		{
			// The way the context is set is different between Silverlight and NUnit,
			// therefor you must do this to correctly fetch the context for each framework.
			// This only applies to asynchronous tests. If you have an asynchronous test
			// you must inherit from TestBase. If you inhert from TestBase all tests in this
			// class must use the context.
			using (UnitTestContext context = GetContext())
			{
				int actual = 0;
				int expected = 1;

				// Simulate asynchronous task
				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += (o, e) =>
				                 	{
				                 		actual = 1;                                 // this will be run in a background thread.
				                 	};
				worker.RunWorkerCompleted += (o, e) =>
				                             	{
				                             		context.Assert.AreEqual(expected, actual);  // Asserts on the context will not throw exceptions.
				                             		context.Assert.Success();                   // notify the context this thread has passed
				                             	};
				worker.RunWorkerAsync();


				// In Silverlight disposing does nothing. In .NET this will cause the
				// test thread to wait for the context to trigger the test thread.
				// You must either fail an Assert(), throw an exception in a Try(), 
				// or call Fail(), Success() or Indeterminate() explicitly
				// to trigger the unit test thread to complete.
			}
		}

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExpectedExceptionExample()
        {
            using (UnitTestContext context = GetContext())
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.RunWorkerCompleted += (o, e) =>
                {
                    // catches exception here and passes to the context.
                    context.Assert.Try(() => { throw new InvalidOperationException(); });
                    context.Assert.Fail();
                };
                worker.RunWorkerAsync();

                // When the context is disposed it will find the exception and re-throw it in .NET
                // and simply pass it back in Silverlight.
            }
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void ExpectedException_After_some_time_Example()
        {
            using (UnitTestContext context = GetContext())
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.RunWorkerCompleted += (o, e) =>
                {
                    //Thread.Sleep(1000);
                    // catches exception here and passes to the context.
                    context.Assert.Try(() => { throw new InvalidOperationException(); });
                    context.Assert.Fail();
                };
                worker.RunWorkerAsync();

                // When the context is disposed it will find the exception and re-throw it in .NET
                // and simply pass it back in Silverlight.
            }
        }

		[TestMethod]
		public void FailureExample()
		{
			using (UnitTestContext context = GetContext())
			{
				int actual = 0;
				int expected = 1;

				BackgroundWorker worker = new BackgroundWorker();
				worker.DoWork += (o, e) =>
				                 	{
				                 		actual = -1;                                // invalid value
				                 	};
				worker.RunWorkerCompleted += (o, e) =>
				                             	{
				                             		// The failed Assert will pass failed test information to the context.
				                             		context.Assert.AreEqual(expected, actual);

				                             		// This will be called but after the failure triggered the context
				                             		// so will be ignored.
				                             		context.Assert.Success();
				                             	};
				worker.RunWorkerAsync();

				// The disposed context will see the failed test and will pass it back 
				// in Silverlight and throw an exception in .NET
			}
		}

        [TestMethod]
        public void Async_test_should_timeout()
        {
            using (UnitTestContext context = GetContext())
            {
                BackgroundWorker worker = new BackgroundWorker();
                worker.DoWork += (o, e) =>
                {
                    Thread.Sleep(6000);
                };
                worker.RunWorkerCompleted += (o, e) =>
                {
                    // The failed Assert will pass failed test information to the context.
                    context.Assert.Fail();
                };
                worker.RunWorkerAsync();

                // The disposed context will see the failed test and will pass it back 
                // in Silverlight and throw an exception in .NET
            }
        }
	}
}
