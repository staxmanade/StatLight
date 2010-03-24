using System.IO;
using NUnit.Framework;
using StatLight.Console;

namespace StatLight.Core.Tests.Console
{
	namespace ArgOptionsTests
	{
		[TestFixture]
		public class when_testing_the_arg_options : FixtureBase
		{
			string samplePath = @"C:\Path\someTest.xap";

			[Test]
			public void when_validating_the_default_properties_of_the_ArgOptions()
			{
				var value = @"C:\Path\someTest.xap";
				var argOptions = GetArgOptions("-x", value);

				argOptions.ContinuousIntegrationMode.ShouldBeFalse();
				argOptions.ShowHelp.ShouldBeFalse();
				argOptions.TagFilters.ShouldEqual(null);
				//argOptions.IsValid.ShouldBeTrue();
			}

			[Test]
			public void should_be_able_to_get_the_xap_path_property_when_given_the_argument()
			{
				var argOptions = GetArgOptions("-x", samplePath);

				argOptions.XapPath.ShouldEqual(samplePath);
			}

			[Test]
			public void when_giving_the_xml_output_file_should_validate_theres_a_directory_to_write_the_file_to()
			{
				typeof(DirectoryNotFoundException).ShouldBeThrownBy(() => GetArgOptions("-x", samplePath, @"-r=C:\some123456DirShouldntExist\Test.xml"));
			}

			[Test]
			public void when_giving_the_xml_output_file_should_validate_theres_a_directory_to_write_the_file_to_1()
			{
				var path = @"C:\Test.xml";
				var options = GetArgOptions("-x", samplePath, @"-r=" + path);
				options.XmlReportOutputPath.ShouldEqual(path);
			}

			[Test]
			public void when_giving_an_empty_tagFilter_arg_it_should_just_use_nothing()
			{
				var argOptions = GetArgOptions("-x", samplePath, "-t");

				argOptions.TagFilters.ShouldBeNull();
			}

			[Test]
			public void should_be_able_to_get_the_tagFilter_property_when_given_the_argument()
			{
				var someTag = "SomeTag";
				var argOptions = GetArgOptions("-x", samplePath, "-t" + someTag);

				argOptions.TagFilters.ShouldEqual(someTag);
			}


			[Test]
			public void should_be_able_to_set_the_WebServerOnly_flag()
			{
				//var argOptions = GetArgOptions("-x", samplePath, "--WebServerOnly");
				var argOptions = GetArgOptions("-x", samplePath, "--webserveronly");

				argOptions.StartWebServerOnly.ShouldBeTrue();
			}

			[Test]
			public void should_be_able_to_set_the_ContinuousIntegrationMode_flag()
			{
				var argOptions = GetArgOptions("-x", samplePath, "-c");

				argOptions.ContinuousIntegrationMode.ShouldBeTrue();
			}

			private ArgOptions GetArgOptions(params string[] args)
			{
				return new ArgOptions(args);
			}
		}
	}
}
