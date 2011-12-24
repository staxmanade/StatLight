using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight
{
    [TestClass]
    public class LongRunningAsyncTests : SilverlightTest
    {
        [TestMethod]
        [Asynchronous]
        [Timeout(120000)]
        public void Should_take_a_really_long_time_and_probably_timeout()
        {
            // never actually call test complete.
        }
    }
}