using System;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using StatLight.Core.UnitTestProviders;
using System.Collections.Generic;
using System.Reflection;
#if SILVERLIGHT
using StatLight.Client.Harness.Hosts;
#endif
namespace StatLight.Core.Configuration
{
    [DataContract]
    public class ClientTestRunConfiguration
    {
        private string _tagFilters = string.Empty;
        private List<string> _methodsToTest;

        public ClientTestRunConfiguration(UnitTestProviderType unitTestProviderType, List<string> methodsToTest, string tagFilters, int numberOfBrowserHosts)
        {
            if (methodsToTest == null) throw new ArgumentNullException("methodsToTest");
            if (unitTestProviderType == UnitTestProviderType.Undefined)
                throw new ArgumentException("Must be defined", "unitTestProviderType");

            if (numberOfBrowserHosts <= 0)
                throw new ArgumentOutOfRangeException("numberOfBrowserHosts", "Must be greater than 0");

            _methodsToTest = methodsToTest;
            _tagFilters = tagFilters ?? string.Empty;
            UnitTestProviderType = unitTestProviderType;
            NumberOfBrowserHosts = numberOfBrowserHosts;
        }

        [DataMember]
        public int NumberOfBrowserHosts { get; set; }


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

        [DataMember]
        public List<string> MethodsToTest
        {
            get { return (_methodsToTest ?? (_methodsToTest = new List<string>())); }
            set { _methodsToTest = value; }
        }


#if SILVERLIGHT
        private static ClientTestRunConfiguration _currentClientTestRunConfiguration;

        public static ClientTestRunConfiguration CurrentClientTestRunConfiguration
        {
            get { return _currentClientTestRunConfiguration; }
            set
            {
                _currentClientTestRunConfiguration = value;

                //var expectedTestsToFindAndRunMessage = string.Join(
                //    " *** Method Filter: {0}".FormatWith(Environment.NewLine),
                //            _currentClientTestRunConfiguration.MethodsToTest.ToArray());
                //StatLight.Client.Harness.Server.Debug(expectedTestsToFindAndRunMessage);

            }
        }

        private static int? _instanceNumber;

        private static readonly IEqualityComparer<string> _ignoreCaseStringComparer =
            StringComparer.Create(CultureInfo.InvariantCulture, true);

        public static bool ContainsMethod(MemberInfo memberInfo)
        {
            if (CurrentClientTestRunConfiguration == null)
                return false;
            if (memberInfo == null)
                throw new ArgumentNullException("memberInfo");

            var methodName = memberInfo.FullName();
            if (CurrentClientTestRunConfiguration.MethodsToTest.Count == 0)
                return ShouldItBeRunInThisInstance(methodName);

            var containsMethod = CurrentClientTestRunConfiguration.MethodsToTest.Contains(methodName, _ignoreCaseStringComparer);
            return containsMethod;
        }

        public static int InstanceNumber
        {
            get
            {
                if (!_instanceNumber.HasValue)
                {
                    string initParam = System.Windows.Application.Current.Host.InitParams["InstaneNumber"];
                    _instanceNumber = int.Parse(initParam);
                }
                return _instanceNumber.Value;
            }
        }

        private static bool ShouldItBeRunInThisInstance(string methodName)
        {
            if (CurrentClientTestRunConfiguration.NumberOfBrowserHosts <= 1)
            {
                return true;
            }

            int methodNameHashCode = Math.Abs(methodName.GetHashCode());

            int moddedHash = methodNameHashCode % CurrentClientTestRunConfiguration.NumberOfBrowserHosts;

            return moddedHash == InstanceNumber;
        }
#endif
    }
}
