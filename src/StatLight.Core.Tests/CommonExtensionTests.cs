using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using System.IO;
using StatLight.Core.Serialization;
using System.Runtime.Serialization;

namespace StatLight.Core.Tests
{
	namespace CommonExtensionTests
	{
	    [TestFixture]
		public class when_working_with_streams
		{
			[Test]
			public void should_be_able_to_convert_a_string_to_a_stream()
			{
				var value = "someString";

				var stream = value.ToStream();

				(new StreamReader(stream)).ReadToEnd()
					.ShouldEqual(value);
			}
		}

		[DataContract]
		public class testSerializable
		{
			[DataMember]
			public int Value { get; set; }
		}

		[TestFixture]
		public class when_seriaizing_and_deserializing
		{

			string serializedString = @"<testSerializable xmlns=""http://schemas.datacontract.org/2004/07/StatLight.Core.Tests.CommonExtensionTests"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Value>10</Value></testSerializable>";

			[Test]
			public void should_be_able_to_serialize_a_test_object()
			{
				var o = new testSerializable() { Value = 10 };
				o.Serialize().ShouldEqual(serializedString);
			}

			[Test]
			public void should_be_able_to_deserialize_a_test_string_into_the_test_object()
			{
				serializedString.Deserialize<testSerializable>()
					.ShouldBeOfType(typeof(testSerializable));
			}
		}

		[TestFixture]
		public class when_testing_the_set_to_Midnight_extension
		{
			[Test]
			public void nows_time_should_be_set_to_midnight()
			{
				DateTime midnight = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
				DateTime.Now.ToMidnight().ShouldEqual(midnight); ;
			}
		}

		[TestFixture]
		public class attribute_finder
		{
			[Test]
			public void should_find_attribute_by_name()
			{
				GetTestMethods(typeof(JJTest)).Count.ShouldEqual(1);
			}

			public ICollection<System.Reflection.MethodInfo> GetTestMethods(Type type)
			{
				var c = new List<System.Reflection.MethodInfo>();
				foreach (var method in type.GetMethods())
					if (method.GetCustomAttributes(true)
						.Where(w => w.ToString().Contains("Fact")).Count() > 0)
						c.Add(method);
				return c;
			}
		}
	}

	public class FactAttribute : Attribute
	{
	}

	public class JJTest
	{
		[Fact]
		public void test1()
		{
		}
	}
}
