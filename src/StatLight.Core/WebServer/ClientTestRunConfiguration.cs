using System;

namespace StatLight.Core.WebServer
{
    using System.Runtime.Serialization;
    using StatLight.Core.UnitTestProviders;
    using System.Collections.Generic;
    using System.Reflection;
    using StatLight.Client.Harness;

    [DataContract]
    public class ClientTestRunConfiguration
    {
        private string _tagFilters = string.Empty;

        [DataMember]
        public string TagFilter
        {
            get
            {
                return _tagFilters;
            }
            set
            {
                if (value == null)
                    _tagFilters = string.Empty;
                else
                    _tagFilters = value;
            }
        }

        [DataMember]
        public UnitTestProviderType UnitTestProviderType { get; set; }

        private List<string> _methodsToTest;

        [DataMember]
        public List<string> MethodsToTest
        {
            get { return (_methodsToTest ?? (_methodsToTest = new List<string>())); }
            set { _methodsToTest = value; }
        }

        public static ClientTestRunConfiguration CreateDefault()
        {
            return new ClientTestRunConfiguration
            {
                TagFilter = string.Empty,
                UnitTestProviderType = UnitTestProviderType.MSTest,
            };
        }

        public static ClientTestRunConfiguration CurrentClientTestRunConfiguration { get; set; }
        public static bool ContainsMethod(MethodInfo methodInfo)
        {
            if (CurrentClientTestRunConfiguration == null)
                return false;
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            if (CurrentClientTestRunConfiguration.MethodsToTest.Count == 0)
                return true;

            string methodString = methodInfo.DeclaringType.FullName + "." + methodInfo.Name;
            if (CurrentClientTestRunConfiguration.MethodsToTest.Contains(methodString))
                return true;

            return false;
        }
    }
}
