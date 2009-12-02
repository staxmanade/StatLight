using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace StatLight.Core.Common
{
	/// <summary>
	/// Simple helper class for argument checks.
	/// </summary>
	public static class Ensure
	{
		/// <summary>
		/// Helper method which throws an <see cref="ArgumentNullException" /> if the specified value is <c>null</c>.
		/// </summary>
		/// <typeparam name="T">The type of the argument.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <param name="name">The name of the argument which is checked.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// Thrown when <paramref name="value"/> is <c>null</c>.
		/// </exception>
		public static void ArgumentIsNotNull<T>(T value, string name)
		{
			if (value == null)
			{
				throw new ArgumentNullException(name);
			}
		}

		/// <summary>
		/// Helper method which throws an <see cref="ArgumentNullException" /> if the specified value is <c>null</c> or zero.
		/// </summary>
		/// <typeparam name="T">The type of the argument.</typeparam>
		/// <param name="value">The value to check.</param>
		/// <param name="name">The name of the argument which is checked.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// Thrown when <paramref name="value"/> is <c>null</c> or zero.
		/// </exception>
		public static void ArgumentIsNotNullOrDefault<T>(T value, string name)
		{
			if (value == null || Equals(value, default(T)))
			{
				throw new ArgumentNullException(name);
			}
		}

		/// <summary>
		/// Helper method which throws an <see cref="ArgumentException" /> if the specified value is uninitialized (meaning holding
		/// the default value).
		/// </summary>
		/// <typeparam message="T">The type of the argument.</typeparam>
		/// <param message="value">The value to check.</param>
		/// <param name="message">The name of the argument which is checked.</param>
		/// <exception cref="T:System.ArgumentException">
		/// Thrown when <paramref message="value"/> is <c>null</c> for reference types, or zero for value types.
		/// </exception>
		public static void ArgumentIsNotDefault<T>(T value, string message)
		{
			if (Equals(value, default(T)))
			{
				if (!typeof(T).IsEnum)
				{
					throw new ArgumentException(message);
				}
			}
		}

		/// <summary>
		/// Checks a string argument to ensure it isn't null or empty.
		/// </summary>
		/// <param name="value">The argument value to check.</param>
		/// <param name="message">The name of the argument.</param>
		/// <exception cref="T:System.ArgumentNullException">
		/// Thrown when <paramref message="value"/> is <c>null</c>.
		/// </exception>
		/// <exception cref="T:System.ArgumentException">
		/// Thrown when <paramref message="value"/> contains an empty
		/// <see cref="string"/>.
		/// </exception>
		public static void ArgumentIsNotNullOrEmptyString(string value, string message)
		{
			ArgumentIsNotNull(value, message);

			if (String.IsNullOrEmpty(value))
			{
				throw new ArgumentException(message);
			}
		}

		/// <summary>
		/// Will throw exception of type <typeparamref name="TException"/> with the specified message if the assertion is false.
		/// </summary>
		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1004:GenericMethodsShouldProvideTypeParameter")]
		public static void That<TException>(bool assertion, string message) where TException : Exception
		{
			if (assertion)
			{
				return;
			}

			throw (TException)Activator.CreateInstance(typeof(TException), message);
		}
	}
}
