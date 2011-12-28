using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Core.Events;
using StatLight.Client.Harness.Hosts;

namespace StatLight.Client.Harness.Hosts.MSTest.LogMessagTranslation
{
    public static class ReflectionInfoHelper
    {
        public static void AssignTestExecutionMethodInfo(this TestExecutionMethodClientEvent testExecutionMethodClientEvent, ITestMethod testMethod)
        {
            var methodInfo = testMethod.Method;
            testExecutionMethodClientEvent.NamespaceName = methodInfo.ReflectedType.Namespace;
            testExecutionMethodClientEvent.ClassName = methodInfo.ReflectedType.ClassNameIncludingParentsIfNested();
            testExecutionMethodClientEvent.MethodName = testMethod.Name;
        }

        public static void AssignMetadata(this TestExecutionMethodClientEvent testExecutionMethodClientEvent, MethodInfo methodInfo)
        {
            var descriptionAttribute = methodInfo.GetAttribute<DescriptionAttribute>().FirstOrDefault();
            if (descriptionAttribute != null)
            {
                testExecutionMethodClientEvent.AddMetadata("Description", descriptionAttribute.Description, "Description");
            }

            var ownerAttribute = methodInfo.GetAttribute<OwnerAttribute>().FirstOrDefault();
            if (ownerAttribute != null)
            {
                testExecutionMethodClientEvent.AddMetadata("Owner", ownerAttribute.Owner, "Owner");
            }

            foreach (var testPropertyAttribute in methodInfo.GetAttribute<TestPropertyAttribute>())
            {
                testExecutionMethodClientEvent.AddMetadata("TestProperty", testPropertyAttribute.Name, testPropertyAttribute.Value);
            }
        }

        private static IEnumerable<T> GetAttribute<T>(this MethodInfo methodInfo)
        {
            return methodInfo
                .GetCustomAttributes(typeof (T), true)
                .Cast<T>();
        }
    }
}