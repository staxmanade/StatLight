using System;
using System.Linq;
using System.Reflection;

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
    }
}