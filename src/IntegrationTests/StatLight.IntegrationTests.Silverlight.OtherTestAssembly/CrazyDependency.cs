using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight.OtherTestAssembly
{
    [TestClass]
    public class CrazyDependency
    {
        /*
         * The purpose of this test is to prove that the 
         * StatLight dll runner can detect and pull in 
         * a non system dependent Assembly
         * 
         */


        [TestMethod]
        public void Should_have_pulled_in_crazy_dependency()
        {
            var type = typeof (Raven.Tests.Silverlight.UnitTestProvider.AsynchronousTaskTest);
            Assert.IsNotNull(type);
        }
    }
}