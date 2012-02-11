using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight
{
    [TestClass]
    public class FirstTestShouldOnlyRunOnce
    {
        private static int _value = 0;

        [TestMethod]
        public void This_test_should_only_run_once()
        {
            _value++;
            Assert.AreEqual(1, _value);
        }
    }
}
