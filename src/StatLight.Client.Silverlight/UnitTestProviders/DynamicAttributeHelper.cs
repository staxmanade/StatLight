namespace StatLight.Client.Silverlight.UnitTestProviders
{
	using System;
	using System.Linq;
	using System.Reflection;

	public static class DynamicAttributeHelper
	{
		public static bool HasAttribute(this MemberInfo method, string attributeName)
		{
			return GetAttribute(method, attributeName) != null;
		}

		public static object GetAttribute(this MemberInfo method, string attributeName)
		{
			return method.GetCustomAttributes(true)
					.Where(w => w.ToString().Contains(attributeName)).FirstOrDefault();
		}

		public static object GetObjectPropertyValue(this object attributeInstance, string propertyName)
		{
			object value = null;

			if (attributeInstance != null)
			{
				var property = attributeInstance
					.GetType()
					.GetProperties()
					.Where(f => f.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))
					.FirstOrDefault();

				if (property != null)
					value = property.GetValue(attributeInstance, null);
			}

			return value;
		}
	}

}
