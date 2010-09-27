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
        public string Description { get; set; }
    }


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCaseAttribute : Attribute
    {
        private readonly object[] _arguments;

        public TestCaseAttribute(params object[] arguments)
        {
            _arguments = arguments ?? new object[] { null };
        }

        public object[] Arguments
        {
            get { return _arguments; }
        }

        public object Result { get; set; }

        public string Description { get; set; }

        public string TestName { get; set; }
    }
}