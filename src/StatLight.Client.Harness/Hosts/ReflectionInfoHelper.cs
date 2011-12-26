using System;
using System.Reflection;
using StatLight.Core.Events.Messaging;

namespace StatLight.Core.Events.Hosts
{
    public static class ReflectionInfoHelper
    {
        public static string ClassNameIncludingParentsIfNested(this Type type)
        {
            var @namespace = type.Namespace ?? string.Empty;
            if (@namespace.Length > 0)
                return type.FullName.Substring(@namespace.Length + 1);

            return type.FullName;
        }

        public static string FullName(this MemberInfo methodInfo)
        {
            string m = "{0}.{1}.{2}".FormatWith(
                        methodInfo.ReflectedType.Namespace,
                        methodInfo.ReflectedType.ClassNameIncludingParentsIfNested(),
                        methodInfo.Name);
            return m;
        }


        public static void HandleReflectionTypeLoadException(ReflectionTypeLoadException rfex)
        {
            string loaderExceptionMessages = "";
            //string msg = "********************* " + helperMessage + "*********************";
            foreach (var t in rfex.LoaderExceptions)
            {
                loaderExceptionMessages += "   -  ";
                loaderExceptionMessages += t.Message;
                loaderExceptionMessages += Environment.NewLine;
            }

            string msg = @"
********************* ReflectionTypeLoadException *********************
***** Begin Loader Exception Messages *****
{0}
***** End Loader Exception Messages *****
".FormatWith(loaderExceptionMessages);

            Server.Trace(msg);
        }
    }
}