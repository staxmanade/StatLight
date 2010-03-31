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
            protected override void Before_all_tests()
            {
                base.Before_all_tests();
                _actualFile = Path.GetTempFileName();
            }

            string _fileNotFound = @"C:\Path\someTestX123456789.xap";
            private string _actualFile;

            [Test]
            public void Should_throw_FileNotFoundException_if_xap_cannot_be_found()
            {
                typeof(FileNotFoundException).ShouldBeThrownBy(() => GetArgOptions("-x", _fileNotFound));
            }

            [Test]
            public void when_validating_the_default_properties_of_the_ArgOptions()
            {
                var argOptions = GetArgOptions("-x", _actualFile);

                argOptions.ContinuousIntegrationMode.ShouldBeFalse();
                argOptions.ShowHelp.ShouldBeFalse();
                argOptions.TagFilters.ShouldEqual(null);
                //argOptions.IsValid.ShouldBeTrue();
            }

            [Test]
            public void should_be_able_to_get_the_xap_path_property_when_given_the_argument()
            {
                var argOptions = GetArgOptions("-x", _actualFile);

                argOptions.XapPath.ShouldEqual(_actualFile);
            }

            [Test]
            public void when_giving_the_xml_output_file_should_validate_theres_a_directory_to_write_the_file_to()
            {
                typeof(DirectoryNotFoundException).ShouldBeThrownBy(() => GetArgOptions("-x", _actualFile, @"-r=C:\some123456DirShouldntExist\Test.xml"));
            }

            [Test]
            public void when_giving_the_xml_output_file_should_validate_theres_a_directory_to_write_the_file_to_1()
            {
                var path = @"C:\Test.xml";
                var options = GetArgOptions("-x", _actualFile, @"-r=" + path);
                options.XmlReportOutputPath.ShouldEqual(path);
            }

            [Test]
            public void when_giving_an_empty_tagFilter_arg_it_should_just_use_nothing()
            {
                var argOptions = GetArgOptions("-x", _actualFile, "-t");

                argOptions.TagFilters.ShouldBeNull();
            }

            [Test]
            public void should_be_able_to_get_the_tagFilter_property_when_given_the_argument()
            {
                var someTag = "SomeTag";
                var argOptions = GetArgOptions("-x", _actualFile, "-t" + someTag);

                argOptions.TagFilters.ShouldEqual(someTag);
            }


            [Test]
            public void should_be_able_to_set_the_WebServerOnly_flag()
            {
                //var argOptions = GetArgOptions("-x", samplePath, "--WebServerOnly");
                var argOptions = GetArgOptions("-x", _actualFile, "--webserveronly");

                argOptions.StartWebServerOnly.ShouldBeTrue();
            }

            [Test]
            public void should_be_able_to_set_the_ContinuousIntegrationMode_flag()
            {
                var argOptions = GetArgOptions("-x", _actualFile, "-c");

                argOptions.ContinuousIntegrationMode.ShouldBeTrue();
            }

            private ArgOptions GetArgOptions(params string[] args)
            {
                return new ArgOptions(args);
            }
        }
    }
}
