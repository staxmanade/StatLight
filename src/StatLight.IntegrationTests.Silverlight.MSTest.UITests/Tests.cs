using System;
using System.Windows;
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


        [AssemblyInitialize]
        public void Setup_StyleManager()
        {
            RegisterStyles<App>("Styles.xaml");
        }

        private void RegisterStyles<T>(string stylesPath)
        {
            var uri = new Uri(string.Format("/{0};component/{1}", typeof(T).Namespace, stylesPath), UriKind.RelativeOrAbsolute);

            Application.Current.Resources.MergedDictionaries.Add(new ResourceDictionary { Source = uri });
        }

        [TestMethod]
        public void TestMethodTryingToBindXaml()
        {

            var controlWithBorder = new ControlWithBorder();
            TestPanel.Children.Add(controlWithBorder);

            var findName = controlWithBorder.FindName("BorderToTest") as Border;
            Assert.IsNotNull(findName);
            Assert.AreEqual(new CornerRadius(10), findName.CornerRadius);
        }
    }
}