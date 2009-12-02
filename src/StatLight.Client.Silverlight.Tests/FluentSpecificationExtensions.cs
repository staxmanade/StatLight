using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace StatLight.Client.Silverlight.Tests
{
	public static class FluentSpecificationExtensions
	{
		public static void ShouldBeTrue(this bool condition)
		{
			Assert.IsTrue(condition);
		}

		public static void ShouldBeFalse(this bool condition)
		{
			Assert.IsFalse(condition);
		}

		public static T ShouldBeEqualTo<T>(this T actual, T expected)
		{
			Assert.AreEqual(expected, actual);
			return actual;
		}

		public static T ShouldNotBeNull<T>(this T actual)
		{
			Assert.IsNotNull(actual);
			return actual;
		}

		public static T ShouldBeNull<T>(this T actual)
		{
			Assert.IsNull(actual);
			return actual;
		}

		public static T ShouldBeInstanceOfType<T>(this T actual, Type expected)
		{
			Assert.IsInstanceOfType(actual, expected);
			return actual;
		}

		public static T ShouldNotBeInstanceOfType<T>(this T actual, Type expected)
		{
			Assert.IsNotInstanceOfType(actual, expected);
			return actual;
		}
	}

}
