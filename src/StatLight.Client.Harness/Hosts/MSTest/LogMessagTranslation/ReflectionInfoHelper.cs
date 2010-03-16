using System;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public static class ReflectionInfoHelper
    {
        public static string ReadClassName(this Type type)
        {
            return type.FullName.Substring(type.Namespace.Length+1);
        }
    }
}