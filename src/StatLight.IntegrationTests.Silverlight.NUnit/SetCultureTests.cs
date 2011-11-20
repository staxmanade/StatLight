using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace StatLight.IntegrationTests.Silverlight
{
    using System.Globalization;
    using NUnit.Framework;

    [TestFixture]
    public class SetCultureAttributeTests
    {
        private CultureInfo _originalCulture;

        [SetUp]
        public void Setup()
        {
            _originalCulture = CultureInfo.CurrentCulture;
        }

        [Test, SetUICulture("fr-FR")]
        public void SetUICultureOnlyToFrench()
        {
            Assert.AreEqual(CultureInfo.CurrentCulture, _originalCulture, "Culture should not change");
            Assert.AreEqual("fr-FR", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
        }

        [Test, SetUICulture("fr-CA")]
        public void SetUICultureOnlyToFrenchCanadian()
        {
            Assert.AreEqual(CultureInfo.CurrentCulture, _originalCulture, "Culture should not change");
            Assert.AreEqual("fr-CA", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
        }

        [Test, SetUICulture("ru-RU")]
        public void SetUICultureOnlyToRussian()
        {
            Assert.AreEqual(CultureInfo.CurrentCulture, _originalCulture, "Culture should not change");
            Assert.AreEqual("ru-RU", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
        }

        [Test, SetCulture("fr-FR"), SetUICulture("fr-FR")]
        public void SetBothCulturesToFrench()
        {
            Assert.AreEqual("fr-FR", CultureInfo.CurrentCulture.Name, "Culture not set correctly");
            Assert.AreEqual("fr-FR", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
        }

        [Test, SetCulture("fr-CA"), SetUICulture("fr-CA")]
        public void SetBothCulturesToFrenchCanadian()
        {
            Assert.AreEqual("fr-CA", CultureInfo.CurrentCulture.Name, "Culture not set correctly");
            Assert.AreEqual("fr-CA", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
        }

        [Test, SetCulture("ru-RU"), SetUICulture("ru-RU")]
        public void SetBothCulturesToRussian()
        {
            Assert.AreEqual("ru-RU", CultureInfo.CurrentCulture.Name, "Culture not set correctly");
            Assert.AreEqual("ru-RU", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
        }

        [Test, SetCulture("fr-FR"), SetUICulture("fr-CA")]
        public void SetMixedCulturesToFrenchAndUIFrenchCanadian()
        {
            Assert.AreEqual("fr-FR", CultureInfo.CurrentCulture.Name, "Culture not set correctly");
            Assert.AreEqual("fr-CA", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
        }

        [Test, SetCulture("ru-RU"), SetUICulture("en-US")]
        public void SetMixedCulturesToRussianAndUIEnglishUS()
        {
            Assert.AreEqual("ru-RU", CultureInfo.CurrentCulture.Name, "Culture not set correctly");
            Assert.AreEqual("en-US", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
        }


        [Test, SetCulture("ru-RU"), SetUICulture("en-US")]
        [TestCase("Hello", 1, "World")]
        [TestCase("Hello", 1, "World1")]
        [TestCase("Hello", 1, "World2")]
        [TestCase("Hello", 1, "World3")]
        public void SetMixedCulturesToRussianAndUIEnglishUSAndMultipleTestCases(string param1, int param2, string param3)
        {
            Assert.AreEqual("ru-RU", CultureInfo.CurrentCulture.Name, "Culture not set correctly");
            Assert.AreEqual("en-US", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");

            Assert.AreEqual("Hello", param1);
            Assert.AreEqual(1, param2);
            Assert.AreEqual("World", param3.Substring(0, 5));
        }


        [TestFixture, SetCulture("ru-RU"), SetUICulture("ru-RU")]
        public class NestedBehavior
        {
            [Test]
            public void InheritedRussian()
            {
                Assert.AreEqual("ru-RU", CultureInfo.CurrentCulture.Name, "Culture not set correctly");
                Assert.AreEqual("ru-RU", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
            }

            [Test, SetUICulture("fr-FR")]
            public void InheritedRussianWithUIFrench()
            {
                Assert.AreEqual("ru-RU", CultureInfo.CurrentCulture.Name, "Culture not set correctly");
                Assert.AreEqual("fr-FR", CultureInfo.CurrentUICulture.Name, "UICulture not set correctly");
            }
        }
    }
}
