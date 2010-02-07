using System;

namespace NUnit.Framework
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class TestAttribute : Attribute
    {
        private string description;

        /// <summary>
        /// Descriptive text for this test
        /// </summary>
        public string Description
        {
            get { return description; }
            set { description = value; }
        }
    }
}