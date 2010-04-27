using System;
using System.Reflection;

namespace StatLight.Client.Harness.Hosts
{
    public static class ReflectionInfoHelper
    {
        public static string ReadClassName(this Type type)
        {
            return type.FullName.Substring(type.Namespace.Length + 1);
        }

        public static string FullName(this MethodInfo methodInfo)
        {
            string m = "{0}.{1}.{2}".FormatWith(
                        methodInfo.DeclaringType.Namespace,
                        methodInfo.DeclaringType.ReadClassName(),
                        methodInfo.Name);
            return m;
        }
    }
}