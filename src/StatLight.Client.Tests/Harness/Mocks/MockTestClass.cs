using System;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace StatLight.Core.Events.Mocks
{
    public class MockTestClass : ITestClass
    {
        #region ITestClass Members

        public string Namespace
        {
            get { throw new NotImplementedException(); }
        }

        public IAssembly Assembly
        {
            get { throw new NotImplementedException(); }
        }

        public System.Reflection.MethodInfo ClassCleanupMethod
        {
            get { throw new NotImplementedException(); }
        }

        public System.Reflection.MethodInfo ClassInitializeMethod
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.ICollection<ITestMethod> GetTestMethods()
        {
            throw new NotImplementedException();
        }

        public bool Ignore
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { return "Mock_some_test_class_name"; }
        }

        public System.Reflection.MethodInfo TestCleanupMethod
        {
            get { throw new NotImplementedException(); }
        }

        public System.Reflection.MethodInfo TestInitializeMethod
        {
            get { throw new NotImplementedException(); }
        }

        public Type Type
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}


