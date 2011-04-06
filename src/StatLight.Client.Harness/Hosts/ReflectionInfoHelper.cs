using System;
using System.Reflection;
using StatLight.Client.Harness.Events;
using StatLight.Client.Harness.Messaging;

namespace StatLight.Client.Harness.Hosts
{
    public static class ReflectionInfoHelper
    {
        /// <summary>
        /// Namespace.Class+Nested+Nested2 -> Class+Nested+Nested2
        /// </summary>
        public static string ClassNameIncludingParentsIfNested(this Type type)
        {
            int nameStart = type.Namespace != null ? type.Namespace.Length+1 : 0;
            return type.FullName.Substring(nameStart);
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