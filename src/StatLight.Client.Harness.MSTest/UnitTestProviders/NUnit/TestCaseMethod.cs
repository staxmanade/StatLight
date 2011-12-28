using StatLight.Core.Events.Messaging;

namespace StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit
{
    using System.Reflection;
    using System.Text;
    using Microsoft.VisualStudio.TestTools.UnitTesting;

    public class TestCaseMethod : TestMethod
    {
        private readonly string _testName;
        private readonly object _expectedResult;
        private readonly object[] _testCaseArguments;

        public TestCaseMethod(MethodInfo methodInfo, object testCaseAttribute)
            : base(methodInfo)
        {
            _testCaseArguments = testCaseAttribute.GetObjectPropertyValue("Arguments") as object[] ?? new object[0];
            _testName = testCaseAttribute.GetObjectPropertyValue("TestName") as string;
            _expectedResult = testCaseAttribute.GetObjectPropertyValue("Result");

            if (_testCaseArguments.Length == 0)
                Server.Debug("TestCaseMethod[{0}] could not get Arguments".FormatWith(methodInfo.Name));
        }

        public override void Invoke(object instance)
        {
            var actualResult = InvokeInternal(instance, _testCaseArguments);

            if (Method.ReturnType != typeof(void))
            {
                Assert.AreEqual(_expectedResult, actualResult);
            }
        }

        public override string Name
        {
            get
            {
                if (!string.IsNullOrEmpty(_testName))
                    return _testName;

                var sb = new StringBuilder();
                sb.Append(base.Name);
                sb.Append("(");
                for (int i = 0; i < _testCaseArguments.Length; i++)
                {
                    var arg = _testCaseArguments[i];
                    string argString = "<null>";
                    if (arg != null)
                        argString = arg.ToString();
                    if (arg is string)
                    {
                        argString = "\"" + argString + "\"";
                    }

                    if (i == _testCaseArguments.Length - 1)
                    {
                        sb.Append(argString);
                    }
                    else
                    {
                        sb.Append(argString + ", ");
                    }
                }
                sb.Append(")");
                return sb.ToString();
            }
        }
    }
}