using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight
{
    [TestClass]
    public class SampleResourceFileTests
    {
        [TestMethod]
        public void Should_be_able_to_read_the_local_xml_file_in_the_xap_under_test()
        {
            XElement xElement = XElement.Load("SampleXmlFile.xml");
            Assert.IsTrue(xElement.ToString().IndexOf("World") >= 0);
        }

        [TestMethod]
        public void Should_be_able_to_read_the_local_xml_file_in_the_xap_under_test_contianed_within_nested_folders()
        {
            XElement xElement = XElement.Load("NestedFolder1/NestedFolder2/SampleXmlFile.xml");
            Assert.IsTrue(xElement.ToString().IndexOf("World") >= 0);
        }
    }
}