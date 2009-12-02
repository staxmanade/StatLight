using System.Linq;
using System.Reflection;

namespace Microsoft.Silverlight.Testing.UnitTesting.Metadata.XunitLight
{
	public class XUnitAttributeNames
	{
		public static readonly string Fact = "Fact";
	}

	public class DynamicAttributeHelper
	{
		public static bool HasAttribute(MethodInfo method, string attributeName)
		{
			return GetAttribute(method, attributeName) != null;
		}

		public static object GetAttribute(MethodInfo method, string attributeName)
		{
			return method.GetCustomAttributes(true)
					.Where(w => w.ToString().Contains(attributeName)).FirstOrDefault();
		}
	}
}
