using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class TestFixtureAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestFixtureSetUpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestFixtureTearDownAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SetUpAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TearDownAttribute : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestAttribute : Attribute
    {
        private string description;

        /// <summary>
        /// Descriptive text for this test
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}