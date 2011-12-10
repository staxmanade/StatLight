using System.Diagnostics;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight
{
    [TestClass]
    [Tag("DebugAssert")]
    [Tag("UI")]
    public class When_calling_debug_assert_with_each_overload
    {
        [TestMethod]
        public void debug_assert_overload_1()
        {
            Debug.Assert(false);
        }

        [TestMethod]
        public void debug_assert_overload_2()
        {
            Debug.Assert(false, "Message 1");
        }

        [TestMethod]
        public void debug_assert_overload_3()
        {
            Debug.Assert(false, "Message 1", "detail message");
        }

        [TestMethod]
        public void debug_assert_overload_4()
        {
            Debug.Assert(false, "Message 1", "overload with message format {0}, {1}, {2}", 1, 2, 3);
        }
    }
}
