using System.Collections.Generic;
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
            var descriptionAttribute = methodInfo.GetAttribute<DescriptionAttribute>().FirstOrDefault();
            if (descriptionAttribute != null)
            {
                testExecutionMethod.AddMetadata("Description", descriptionAttribute.Description, "Description");
            }

            var ownerAttribute = methodInfo.GetAttribute<OwnerAttribute>().FirstOrDefault();
            if (ownerAttribute != null)
            {
                testExecutionMethod.AddMetadata("Owner", ownerAttribute.Owner, "Owner");
            }

            foreach (var testPropertyAttribute in methodInfo.GetAttribute<TestPropertyAttribute>())
            {
                testExecutionMethod.AddMetadata("TestProperty", testPropertyAttribute.Name, testPropertyAttribute.Value);
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