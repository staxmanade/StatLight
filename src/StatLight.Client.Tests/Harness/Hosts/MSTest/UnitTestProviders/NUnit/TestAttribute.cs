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


    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class TestCaseAttribute : Attribute
    {
        private object[] arguments;
        private object result;
        private Type expectedExceptionType;
        private string expectedExceptionName;
        private string expectedMessage;
        private string description;
        private string testName;
        private bool isIgnored;
        private string ignoreReason;

        /// <summary>
        /// Construct a TestCaseAttribute with a list of arguments.
        /// This constructor is not CLS-Compliant
        /// </summary>
        /// <param name="arguments"></param>
        public TestCaseAttribute(params object[] arguments)
        {
            if (arguments == null)
                this.arguments = new object[] { null };
            else
                this.arguments = arguments;
        }

        /// <summary>
        /// Construct a TestCaseAttribute with a single argument
        /// </summary>
        /// <param name="arg"></param>
        public TestCaseAttribute(object arg)
        {
            this.arguments = new object[] { arg };
        }

        /// <summary>
        /// Construct a TestCaseAttribute with a two arguments
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        public TestCaseAttribute(object arg1, object arg2)
        {
            this.arguments = new object[] { arg1, arg2 };
        }

        /// <summary>
        /// Construct a TestCaseAttribute with a three arguments
        /// </summary>
        /// <param name="arg1"></param>
        /// <param name="arg2"></param>
        /// <param name="arg3"></param>
        public TestCaseAttribute(object arg1, object arg2, object arg3)
        {
            this.arguments = new object[] { arg1, arg2, arg3 };
        }

        /// <summary>
        /// Gets the list of arguments to a test case
        /// </summary>
        public object[] Arguments
        {
            get { return arguments; }
        }

        /// <summary>
        /// Gets or sets the expected result.
        /// </summary>
        /// <value>The result.</value>
        public object Result
        {
            get { return result; }
            set { result = value; }
        }

        /// <summary>
        /// Gets or sets the expected exception.
        /// </summary>
        /// <value>The expected exception.</value>
        public Type ExpectedException
        {
            get { return expectedExceptionType; }
            set
            {
                expectedExceptionType = value;
                expectedExceptionName = expectedExceptionType.FullName;
            }
        }

        /// <summary>
        /// Gets or sets the name the expected exception.
        /// </summary>
        /// <value>The expected name of the exception.</value>
        public string ExpectedExceptionName
        {
            get { return expectedExceptionName; }
            set
            {
                expectedExceptionName = value;
                expectedExceptionType = null;
            }
        }

        /// <summary>
        /// Gets or sets the expected message of the expected exception
        /// </summary>
        /// <value>The expected message of the exception.</value>
        public string ExpectedMessage
        {
            get { return expectedMessage; }
            set { expectedMessage = value; }
        }


        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>The description.</value>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }

        /// <summary>
        /// Gets or sets the name of the test.
        /// </summary>
        /// <value>The name of the test.</value>
        public string TestName
        {
            get { return testName; }
            set { testName = value; }
        }

        /// <summary>
        /// Gets or sets the ignored status of the test
        /// </summary>
        public bool Ignore
        {
            get { return isIgnored; }
            set { isIgnored = value; }
        }

        /// <summary>
        /// Gets or sets the ignored status of the test
        /// </summary>
        public bool Ignored
        {
            get { return isIgnored; }
            set { isIgnored = value; }
        }

        /// <summary>
        /// Gets the ignore reason.
        /// </summary>
        /// <value>The ignore reason.</value>
        public string IgnoreReason
        {
            get { return ignoreReason; }
            set
            {
                ignoreReason = value;
                isIgnored = ignoreReason != null && ignoreReason != string.Empty;
            }
        }
    }
}