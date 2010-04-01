
using System;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Silverlight.Testing.UnitTesting.Metadata;

namespace StatLight.Client.Harness.Hosts.MSTest.UnitTestProviders.NUnit
{
    /// <summary>
	/// Expected exception metadata.
	/// </summary>
	[SuppressMessage("Microsoft.Naming", "CA1711:IdentifiersShouldNotHaveIncorrectSuffix", Justification = "Standard unit test framework naming")]
	public class ExpectedException : IExpectedException
	{
		/// <summary>
		/// Private constructor.
		/// </summary>
		private ExpectedException() { }

		/// <summary>
		/// Creates a new expected exception metadata wrapper.
		/// </summary>
		/// <param name="expectedExceptionAttribute">Attribute value.</param>
		public ExpectedException(object expectedExceptionAttribute)
		{
			_exp = expectedExceptionAttribute;
			if (_exp == null)
			{
				throw new ArgumentNullException("expectedExceptionAttribute");
			}
		}

		/// <summary>
		/// The expected exception attribute.
		/// </summary>
		private object _exp;

		/// <summary>
		/// Gets the type of the expected exception.
		/// </summary>
		public Type ExceptionType
		{
			get { return _exp.GetObjectPropertyValue("ExceptionType") as Type; }
		}

		/// <summary>
		/// Gets any message to include in a failure.
		/// </summary>
		public string Message
		{
			get { return _exp.GetObjectPropertyValue("UserMessage") as string; }
		}
	}

}
