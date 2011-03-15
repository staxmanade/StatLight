using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight.OtherTestAssembly
{
    [TestClass]
    public class CrazyDependency
    {
        /*
         * The purpose of this test is to prove that the 
         * StatLight dll runner can detect and pull in 
         * the FSharep.Core dependent Assembly
         * 
         */


        [TestMethod]
        public void Should_have_pulled_in_crazy_dependency()
        {
            var type = typeof (System.Threading.Tasks.Task);
            Assert.IsNotNull(type);
        }
    }
}