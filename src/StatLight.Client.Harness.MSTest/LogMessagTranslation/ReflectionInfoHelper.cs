using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public static class ReflectionInfoHelper
    {
        public static void AssignTestExecutionMethodInfo(this TestExecutionMethod testExecutionMethod, ITestMethod testMethod)
        {
            var methodInfo = testMethod.Method;
            testExecutionMethod.NamespaceName = methodInfo.ReflectedType.Namespace;
            testExecutionMethod.ClassName = methodInfo.ReflectedType.ClassNameIncludingParentsIfNested();
            testExecutionMethod.MethodName = testMethod.Name;
        }
    }
}