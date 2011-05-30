using System;
using System.Linq;
using System.Reflection;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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

        public static void AssignMetadata(this TestExecutionMethod testExecutionMethod, MethodInfo methodInfo)
        {
            ReadProperty(methodInfo, typeof(DescriptionAttribute), "Description", testExecutionMethod);
            ReadProperty(methodInfo, typeof(OwnerAttribute), "Owner", testExecutionMethod);
        }

        private static void ReadProperty(MethodInfo methodInfo, Type attributeType, string propertyName, TestExecutionMethod testExecutionMethod)
        {
            var descriptionAttribute = methodInfo
                .GetCustomAttributes(attributeType, true)
                .FirstOrDefault();

            if (descriptionAttribute == null)
            {
                return;
            }

            var value = (string)(attributeType.GetProperty(propertyName).GetValue(descriptionAttribute, new object[0]));

            testExecutionMethod.AddMetadata(propertyName, value);
        }
    }
}