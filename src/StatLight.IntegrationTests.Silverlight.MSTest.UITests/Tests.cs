using System;
using System.Windows.Controls;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace StatLight.IntegrationTests.Silverlight.MSTest.UITests
{
    [TestClass]
    public class Tests : PresentationTest
    {
        [TestMethod]
        [Asynchronous]
        public void TestMethod1()
        {
            var uiElement = new Button {Content = "HELLO"};
            TestPanel.Children.Add(uiElement);
            EnqueueDelay(TimeSpan.FromSeconds(5));
            EnqueueTestComplete();
        }
    }
}