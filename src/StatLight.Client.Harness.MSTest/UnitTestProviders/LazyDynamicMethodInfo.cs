
using System;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace StatLight.Core.Events.Hosts.MSTest.UnitTestProviders
{
    /// <summary>
	/// A class that does a lazy lookup when needed using reflection.
	/// </summary>
	public class LazyDynamicMethodInfo
	{
		/// <summary>
		/// Attribute Type instance.
		/// </summary>
		private string _attributeType;

		/// <summary>
		/// The Type to search with.
		/// </summary>
		private Type _searchType;

		/// <summary>
		/// Whether the search has happened.
		/// </summary>
		private bool _hasSearched;

		/// <summary>
		/// The method reflection object.
		/// </summary>
		private MethodInfo _methodInfo;

		/// <summary>
		/// Construct a new lazy method wrapper.
		/// </summary>
		/// <param name="searchType">Type to search.</param>
		/// <param name="attributeType">Attribute type.</param>
		public LazyDynamicMethodInfo(Type searchType, string attributeType)
		{
			_searchType = searchType;
			_attributeType = attributeType;
		}

		/// <summary>
		/// Gets the type of attribute the lazy method is searching for.
		/// </summary>
		protected string AttributeType
		{
			get { return _attributeType; }
		}

		/// <summary>
		/// Gets the underlying type that is searched.
		/// </summary>
		protected Type SearchType
		{
			get { return _searchType; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether a lookup has already been attempted.
		/// </summary>
		protected bool HasSearched
		{
			get { return _hasSearched; }
			set { _hasSearched = value; }
		}

		/// <summary>
		/// Gets or sets the underlying MethodInfo from reflection.
		/// </summary>
		protected MethodInfo MethodInfo
		{
			get { return _methodInfo; }
			set { _methodInfo = value; }
		}

		/// <summary>
		/// Does a search and retrieves the method information.
		/// </summary>
		/// <returns>The method reflection object.</returns>
		[SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate", Justification = "Helps document the more advanced lazy load functionality to API users")]
		public MethodInfo GetMethodInfo()
		{
			if (!_hasSearched)
			{
				Search();
			}
			return _methodInfo;
		}

		/// <summary>
		/// Whether the type has a method info.
		/// </summary>
		/// <returns>A value indicating whether the method information has 
		/// been found.</returns>
		public bool HasMethodInfo()
		{
			if (!_hasSearched)
			{
				Search();
			}
			return (_methodInfo != null);
		}

		protected virtual void Search()
		{
			Search(_searchType);
		}

		/// <summary>
		/// Perform a search on the type.
		/// </summary>
		protected virtual void Search(Type searchType)
		{
			if (_hasSearched)
			{
				return;
			}

			foreach (var methodInfo in searchType.GetMethods())
				if (methodInfo.HasAttribute(_attributeType))
				{
					_methodInfo = methodInfo;
					break;
				}

			_hasSearched = true;
		}
	}
}
