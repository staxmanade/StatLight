using System;
using System.Reflection;
using StatLight.Client.Harness.Events;

namespace StatLight.Client.Harness.Hosts
{
    public static class ReflectionInfoHelper
    {
        public static string ReadClassName(this Type type)
        {
            return type.FullName.Substring(type.Namespace.Length + 1);
        }

        public static void AssignTestExecutionMethodInfo(this TestExecutionMethod testExecutionMethod, MethodInfo methodInfo)
        {
            testExecutionMethod.NamespaceName = methodInfo.ReflectedType.Namespace;
            testExecutionMethod.ClassName = methodInfo.ReflectedType.ReadClassName();
            testExecutionMethod.MethodName = methodInfo.Name;
        }


        public static string FullName(this MemberInfo methodInfo)
        {
            string m = "{0}.{1}.{2}".FormatWith(
                        methodInfo.ReflectedType.Namespace,
                        methodInfo.ReflectedType.ReadClassName(),
                        methodInfo.Name);
            return m;
        }
    }
}