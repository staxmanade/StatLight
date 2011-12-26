using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Reflection;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using StatLight.Core.Events;
using StatLight.Core.Events.Messaging;

namespace StatLight.Core.Events.Hosts.MSTest.UnitTestProviders.MSTest
{
    /// <summary>
    /// A provider wrapper for a test method.
    /// </summary>
    public class TestMethod : ITestMethod
    {
        /// <summary>
        /// Property name for the TestContext.
        /// </summary>
        private const string ContextPropertyName = "TestContext";

        /// <summary>
        /// Default value for methods when no priority attribute is defined.
        /// </summary>
        private const int DefaultPriority = 3;

        /// <summary>
        /// An empty object array.
        /// </summary>
        private static readonly object[] None = { };

        /// <summary>
        /// Method reflection object.
        /// </summary>
        private readonly MethodInfo _methodInfo;

        /// <summary>
        /// Private constructor, the constructor requires the method reflection object.
        /// </summary>
        private TestMethod() { }

        /// <summary>
        /// Creates a new test method wrapper object.
        /// </summary>
        /// <param name="methodInfo">The reflected method.</param>
        public TestMethod(MethodInfo methodInfo)
            : this()
        {
            _methodInfo = methodInfo;
        }

        /// <summary>
        /// Allows the test to perform a string WriteLine.
        /// </summary>
        public event EventHandler<StringEventArgs> WriteLine;

        /// <summary>
        /// Call the WriteLine method.
        /// </summary>
        /// <param name="s">String to WriteLine.</param>
        internal void OnWriteLine(string s)
        {
            var sea = new StringEventArgs(s);
            if (WriteLine != null)
            {
                WriteLine(this, sea);
            }
        }

        /// <summary>
        /// Decorates a test class instance with the unit test framework's 
        /// specific test context capability, if supported.
        /// </summary>
        /// <param name="instance">Instance to decorate.</param>
        public void DecorateInstance(object instance)
        {
            if (instance == null)
            {
                return;
            }

            Type t = instance.GetType();
            PropertyInfo pi = t.GetProperty(ContextPropertyName, BindingFlags.Public | BindingFlags.Instance);
            if (pi != null && pi.CanWrite)
            {
                var statLightTestContext = new StatLightTestContext(this);
                pi.SetValue(instance, statLightTestContext, null);
            }
        }

        /// <summary>
        /// Gets the underlying reflected method.
        /// </summary>
        public MethodInfo Method
        {
            get { return _methodInfo; }
        }

        /// <summary>
        /// Gets a value indicating whether there is an Ignore attribute.
        /// </summary>
        public bool Ignore
        {
            get { return Method.HasAttribute(ProviderAttributes.IgnoreAttribute); }
        }

        /// <summary>
        /// Gets any description marked on the test method.
        /// </summary>
        public string Description
        {
            get
            {
                string description = null;

                var descriptionAttribute = Method.GetAttribute(ProviderAttributes.DescriptionAttribute);

                if (descriptionAttribute != null)
                    description = descriptionAttribute.GetObjectPropertyValue("Description") as string;

                return description;
            }
        }

        /// <summary>
        /// Gets the name of the method.
        /// </summary>
        public virtual string Name
        {
            get { return _methodInfo.Name; }
        }

        /// <summary>
        /// Gets the Category.
        /// </summary>
        public string Category
        {
            get { return null; }
        }

        /// <summary>
        /// Gets the owner name of the test.
        /// </summary>
        public string Owner
        {
            get
            {
                var owner = Method.GetAttribute(ProviderAttributes.OwnerAttribute);
                return owner == null ? null : owner.GetObjectPropertyValue("Owner") as string;
            }
        }

        /// <summary>
        /// Gets any expected exception attribute information for the test method.
        /// </summary>
        public IExpectedException ExpectedException
        {
            get
            {
                var exp = Method.GetAttribute(ProviderAttributes.ExpectedExceptionAttribute);

                return exp != null ?
                    new ExpectedException(exp) : null;
            }
        }

        /// <summary>
        /// Gets any timeout.  A Nullable property.
        /// </summary>
        public int? Timeout
        {
            get
            {
                var timeoutAttribute = Method.GetAttribute(ProviderAttributes.TimeoutAttribute);
                var timeoutValue = timeoutAttribute == null ? null : timeoutAttribute.GetObjectPropertyValue("Timeout") as int?;
                return timeoutValue;
            }
        }

        /// <summary>
        /// Gets a Collection of test properties.
        /// </summary>
        public ICollection<ITestProperty> Properties
        {
            get { return null; }
        }

        /// <summary>
        /// Gets a collection of test work items.
        /// </summary>
        public ICollection<IWorkItemMetadata> WorkItems
        {
            get { return null; }
        }

        /// <summary>
        /// Gets Priority information.
        /// </summary>
        public IPriority Priority
        {
            get
            {
                var pri = Method.GetAttribute(ProviderAttributes.Priority);
                //VS.PriorityAttribute pri = ReflectionUtility.GetAttribute(this, ProviderAttributes.Priority, true) as VS.PriorityAttribute;
                return new Priority(pri == null ? DefaultPriority : (int)pri.GetObjectPropertyValue("Priority"));
            }
        }

        /// <summary>
        /// Get any attribute on the test method that are provided dynamically.
        /// </summary>
        /// <returns>
        /// Dynamically provided attributes on the test method.
        /// </returns>
        public virtual IEnumerable<Attribute> GetDynamicAttributes()
        {
            return new Attribute[] { };
        }

        /// <summary>
        /// Invoke the test method.
        /// </summary>
        /// <param name="instance">Instance of the test class.</param>
        public virtual void Invoke(object instance)
        {
            _methodInfo.Invoke(instance, None);
        }
    }


    public class StatLightTestContext : TestContext
    {
        private readonly TestMethod _testMethod;
        private readonly Dictionary<string, string> _propertyCache = new Dictionary<string, string>();
        public override IDictionary Properties
        {
            get
            {
                if (_testMethod.Properties != null)
                {
                    foreach (var testProperty in _testMethod.Properties)
                    {
                        _propertyCache.Add(testProperty.Name, testProperty.Value);
                    }
                }

                return _propertyCache;
            }
        }

        public override DataRow DataRow { get { throw NotSupportedException("DataRow"); } }
        public override DbConnection DataConnection { get { throw NotSupportedException("DataConnection"); } }
        public override string TestName { get { return _testMethod.Name; } }
        public override UnitTestOutcome CurrentTestOutcome { get { return UnitTestOutcome.Unknown; } }

        internal StatLightTestContext(TestMethod testMethod)
        {
            _testMethod = testMethod;
        }

        private static int _newOrder = 0;
        private static readonly object _sync = new object();
        public override void WriteLine(string format, params object[] args)
        {
            int order;
            lock (_sync)
            {
                order = ++_newOrder;
            }

            string s = (args.Length == 0) ? format : string.Format(CultureInfo.InvariantCulture, format, args);

            var newMsg = new TestContextMessageClientEvent
                                                       {
                                                           FullTestName = _testMethod.Method.FullName(),
                                                           Message = s,
                                                       };
            newMsg.Order = order;
            Server.PostMessage(newMsg);

            _testMethod.OnWriteLine(s);
        }

        private static Exception NotSupportedException(string functionality)
        {
            return new NotSupportedException(functionality);
        }

        public override void AddResultFile(string fileName)
        {
            throw NotSupportedException("AddResultFile");
        }
        public override void BeginTimer(string timerName)
        {
            throw NotSupportedException("BeginTimer");
        }
        public override void EndTimer(string timerName)
        {
            throw NotSupportedException("EndTimer");
        }
    }
}
