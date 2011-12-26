
using System;
using System.Reflection;

namespace StatLight.Core.Events.Hosts.MSTest.UnitTestProviders
{
    /// <summary>
    /// A lazy method type.
    /// </summary>
    public class LazyDynamicAssemblyMethodInfo
    {
        /// <summary>
        /// Underlying Assembly reflection object.
        /// </summary>
        private readonly Assembly _assembly;

        private readonly string _attributeType;

        /// <summary>
        /// Create a new lazy method from a MethodInfo instance.
        /// </summary>
        /// <param name="assembly">Assembly reflection object.</param>
        /// <param name="attributeType">Attribute Type instance.</param>
        public LazyDynamicAssemblyMethodInfo(Assembly assembly, string attributeType)
        {
            _assembly = assembly;
            _attributeType = attributeType;
        }

        /// <summary>
        /// Performs a search on the MethodInfo for the attributes needed.
        /// </summary>
        protected MethodInfo Search()
        {
            if (_assembly == null)
            {
                return null;
            }

            Type[] types = _assembly.GetExportedTypes();
            foreach (Type type in types)
            {
                foreach (var methodInfo in type.GetMethods())
                    if (methodInfo.HasAttribute(_attributeType))
                    {
                        return methodInfo;
                    }
            }
            return null;
        }

        private bool _searchedAlready;
        private MethodInfo _foundMethodInfo;
        public MethodInfo GetMethodInfo()
        {
            if(_searchedAlready)
                return _foundMethodInfo;

            _foundMethodInfo = Search();
            _searchedAlready = true;
            return _foundMethodInfo;
        }
    }
}
