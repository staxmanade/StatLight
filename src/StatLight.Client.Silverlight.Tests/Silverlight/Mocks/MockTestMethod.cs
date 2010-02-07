using System;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace StatLight.Client.Silverlight.Mocks
{
    public class MockTestMethod : ITestMethod
    {
        #region ITestMethod Members

        public string Category
        {
            get { throw new NotImplementedException(); }
        }

        public void DecorateInstance(object instance)
        {
            throw new NotImplementedException();
        }

        public string Description
        {
            get { throw new NotImplementedException(); }
        }

        public IExpectedException ExpectedException
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.IEnumerable<Attribute> GetDynamicAttributes()
        {
            throw new NotImplementedException();
        }

        public bool Ignore
        {
            get { throw new NotImplementedException(); }
        }

        public void Invoke(object instance)
        {
            throw new NotImplementedException();
        }

        public System.Reflection.MethodInfo Method
        {
            get { throw new NotImplementedException(); }
        }

        public string Name
        {
            get { return "Mock_some_test_method_name"; }
        }

        public string Owner
        {
            get { throw new NotImplementedException(); }
        }

        public IPriority Priority
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.ICollection<ITestProperty> Properties
        {
            get { throw new NotImplementedException(); }
        }

        public int? Timeout
        {
            get { throw new NotImplementedException(); }
        }

        public System.Collections.Generic.ICollection<IWorkItemMetadata> WorkItems
        {
            get { throw new NotImplementedException(); }
        }

        public event EventHandler<StringEventArgs> WriteLine;

        private void InvokeWriteLine(StringEventArgs e)
        {
            EventHandler<StringEventArgs> writeLineHandler = WriteLine;
            if (writeLineHandler != null) writeLineHandler(this, e);
        }

        #endregion
    }
}


