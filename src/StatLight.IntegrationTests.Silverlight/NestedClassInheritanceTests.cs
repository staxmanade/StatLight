using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight
{
    namespace NestedClassInheritanceTests
    {
        [TestClass]
        public class A
        {
            [TestMethod]
            public void MethodA() { }


            [TestClass]
            public class B1 : A
            {

            }


            [TestClass]
            public class B2 : A
            {

            }

        }
    }
}