using System;

namespace StatLight.Client.Harness.ClientEventMapping
{
    public static class ReflectionInfoHelper
    {
        public static string ReadClassName(this Type type)
        {
            return type.FullName.Substring(type.Namespace.Length+1);
        }
    }
}