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

        private static ClientTestRunConfiguration _currentClientTestRunConfiguration;
        public static ClientTestRunConfiguration CurrentClientTestRunConfiguration
        {
            get { return _currentClientTestRunConfiguration; }
            set
            {
                _currentClientTestRunConfiguration = value;

#if SILVERLIGHT
                var expectedTestsToFindAndRunMessage = string.Join(
                    " *** Method Filter: {0}".FormatWith(Environment.NewLine),
                            _currentClientTestRunConfiguration.MethodsToTest.ToArray());
                Server.Debug(expectedTestsToFindAndRunMessage);
#endif

            }
        }

        public static bool ContainsMethod(MethodInfo methodInfo)
        {
            if (CurrentClientTestRunConfiguration == null)
                return false;
            if (methodInfo == null)
                throw new ArgumentNullException("methodInfo");

            if (CurrentClientTestRunConfiguration.MethodsToTest.Count == 0)
                return true;

            string methodString = methodInfo.DeclaringType.FullName + "." + methodInfo.Name;

            var containsMethod = CurrentClientTestRunConfiguration.MethodsToTest.Contains(methodString);

#if SILVERLIGHT
            var expectedTestsToFindAndRunMessage = " *** Contains Method: {0}, {1}".FormatWith(containsMethod, methodString);
            Server.Debug(expectedTestsToFindAndRunMessage);
#endif

            return containsMethod;
        }
    }
}
