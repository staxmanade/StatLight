using System.Reflection;
using System.Text;

namespace StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit
{
    public class TestCaseMethod : TestMethod
    {
        private readonly object[] _testCaseArguments;

        public TestCaseMethod(MethodInfo methodInfo, object[] testCaseArguments)
            : base(methodInfo)
        {
            _testCaseArguments = testCaseArguments;
        }

        public override void Invoke(object instance)
        {
            Method.Invoke(instance, _testCaseArguments);
        }

        public override string Name
        {
            get
            {
                var sb = new StringBuilder();
                sb.Append(base.Name);
                sb.Append("(");
                for (int i = 0; i < _testCaseArguments.Length; i++)
                {
                    var arg = _testCaseArguments[i];
                    string argString = arg.ToString();
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