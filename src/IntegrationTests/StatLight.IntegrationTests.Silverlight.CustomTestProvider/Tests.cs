using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Raven.Tests.Silverlight.UnitTestProvider;

namespace StatLight.IntegrationTests.Silverlight.CustomTestProvider
{
    [TestClass]
    public class Tests : AsynchronousTaskTest
    {
        [TestMethod]
        public void TestMethod1()
        {
        }

        [TestMethod]
        [Asynchronous]
        public IEnumerable<Task> AsyncronousTest()
        {
            var something = SomeTestTask.DoSomethingAsync();
            yield return something;

            var another = SomeTestTask.DoSomethingAsync();
            yield return another;

            //yield return SomeTestTask.DoSomethingAsync().Wait(100);

            Assert.AreEqual(42, another.Result);
        }
    }

    public static class SomeTestTask
    {
        public static Task<int> DoSomethingAsync()
        {
            var tcs = new TaskCompletionSource<int>();
            tcs.TrySetResult(42);
            return tcs.Task;
        }
    }
}