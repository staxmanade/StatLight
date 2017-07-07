using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Client.Harness.Hosts;

public class ClassInGlobalNamespace
{
    public class Nested
    {
        public class NestedAgain { }
    }
}

namespace StatLight.Client.Tests.Harness.Hosts
{
    [TestClass]
    public class ReflectionInfoHelperTest
    {
        [TestMethod]
        public void CanGetNameOfClassInGlobalNamespace()
        {
            //string name = typeof(ClassInGlobalNamespace).ClassNameIncludingParentsIfNested();
            string name = typeof(ClassInGlobalNamespace).ClassNameIncludingParentsIfNested();
            Assert.AreEqual("ClassInGlobalNamespace", name);
        }
        [TestMethod]
        public void ParensOfNestedClassesAreIncluded()
        {
            string name = typeof(ClassInGlobalNamespace.Nested.NestedAgain).ClassNameIncludingParentsIfNested();
            Assert.AreEqual("ClassInGlobalNamespace+Nested+NestedAgain", name);
        }
    }
}
