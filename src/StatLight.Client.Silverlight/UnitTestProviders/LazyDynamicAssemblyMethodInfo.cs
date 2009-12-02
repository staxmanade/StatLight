
namespace StatLight.Client.Silverlight.UnitTestProviders
{
	using System;
	using System.Reflection;
	using System.Diagnostics.CodeAnalysis;

	/// <summary>
	/// A lazy method type.
	/// </summary>
	public class LazyDynamicAssemblyMethodInfo : LazyDynamicMethodInfo
	{
		/// <summary>
		/// Underlying Assembly reflection object.
		/// </summary>
		private Assembly _assembly;

		/// <summary>
		/// Create a new lazy method from a MethodInfo instance.
		/// </summary>
		/// <param name="assembly">Assembly reflection object.</param>
		/// <param name="attributeType">Attribute Type instance.</param>
		public LazyDynamicAssemblyMethodInfo(Assembly assembly, string attributeType)
			: base(assembly.GetType(), attributeType)
		{
			_assembly = assembly;
		}

		/// <summary>
		/// Performs a search on the MethodInfo for the attributes needed.
		/// </summary>
		protected override void Search()
		{
			if (HasSearched)
			{
				return;
			}
			if (_assembly == null)
			{
				return;
			}
			Type[] types = _assembly.GetExportedTypes();
			foreach (Type type in types)
			{
				base.Search(type);

				if (MethodInfo != null)
				{
					break;
				}
			}
			HasSearched = true;
		}
	}
}
