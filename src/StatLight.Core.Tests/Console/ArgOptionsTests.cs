using System.IO;
using System.Linq;
using NUnit.Framework;
using StatLight.Console;
using StatLight.Core.Configuration;

namespace StatLight.Core.Tests.Console
{
    namespace ArgOptionsTests
    {
        public class ArgOptionsTestBase : FixtureBase
        {

        }

        [TestFixture]
        public class when_testing_the_arg_options : ArgOptionsTestBase
        {
            protected override void Before_all_tests()
            {
                base.Before_all_tests();
                _actualFile = Path.GetTempFileName();
            }

            private string _actualFile;

            [Test]
            public void when_validating_the_default_properties_of_the_ArgOptions()
            {
                var argOptions = ("-x=" + _actualFile).ToArgOptions();

                argOptions.ContinuousIntegrationMode.ShouldBeFalse();
                argOptions.ShowHelp.ShouldBeFalse();
                argOptions.TagFilters.ShouldEqual(null);
            }

            [Test]
            public void should_be_able_to_get_the_xap_path_property_when_given_the_argument()
            {
                var argOptions = ("-x" + _actualFile).ToArgOptions();

                argOptions.XapPaths.First().ShouldEqual(_actualFile);
            }

            [Test]
            public void when_giving_the_xml_output_file_should_validate_theres_a_directory_to_write_the_file_to()
            {
                typeof(DirectoryNotFoundException).ShouldBeThrownBy(() => ("-x=" + _actualFile + @" -r=C:\some123456DirShouldntExist\Test.xml").ToArgOptions());
            }

            [Test]
            public void when_giving_the_xml_output_file_should_validate_theres_a_directory_to_write_the_file_to_1()
            {
                var path = @"C:\Test.xml";
                var options = ("-x=" + _actualFile + " -r=" + path).ToArgOptions();
                options.XmlReportOutputPath.ShouldEqual(path);
            }

            [Test]
            public void when_giving_an_empty_tagFilter_arg_it_should_just_use_nothing()
            {
                var argOptions = ("-x=" + _actualFile + " -t").ToArgOptions();

                argOptions.TagFilters.ShouldBeNull();
            }

            [Test]
            public void should_be_able_to_get_the_tagFilter_property_when_given_the_argument()
            {
                var someTag = "SomeTag";
                var argOptions = ("-x=" + _actualFile + " -t" + someTag).ToArgOptions();
                argOptions.TagFilters.ShouldEqual(someTag);
            }


            [Test]
            public void should_be_able_to_set_the_WebServerOnly_flag()
            {
                var argOptions = ("-x=" + _actualFile + " --webserveronly").ToArgOptions();

                argOptions.StartWebServerOnly.ShouldBeTrue();
            }

            [Test]
            public void should_be_able_to_set_the_ContinuousIntegrationMode_flag()
            {
                var argOptions = ("-x " + _actualFile + " -c").ToArgOptions();

                argOptions.ContinuousIntegrationMode.ShouldBeTrue();
            }

            [Test]
            public void Should_be_able_to_set_a_dll_to_test()
            {
                ArgOptions command = "-d:somePath.dll".ToArgOptions();

                command.XapPaths.Count.ShouldEqual(0);

                command.Dlls.Count.ShouldEqual(1);
                command.Dlls.First().ShouldEqual("somePath.dll");
            }
        }

        public class when_specifying_the_BrowserWindow_flag
        {
            [TestCase("", BrowserWindowState.Normal, 800, 600)]
            [TestCase("Maximized", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("Minimized", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("maximized", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("minimized", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("Normal", BrowserWindowState.Normal, 800, 600)]
            [TestCase("normal", BrowserWindowState.Normal, 800, 600)]
            [TestCase("M", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("m", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("N", BrowserWindowState.Normal, 800, 600)]
            [TestCase("800x600", BrowserWindowState.Normal, 800, 600)]
            [TestCase("M800x600", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("Maximized800x600", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("maximized800x600", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("m800x600", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("Minimized800x600", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("minimized800x600", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("N800x600", BrowserWindowState.Normal, 800, 600)]
            [TestCase("Normal80x600", BrowserWindowState.Normal, 800, 600)]
            [TestCase("normal80x600", BrowserWindowState.Normal, 800, 600)]

            [TestCase("\"800x600\"", BrowserWindowState.Normal, 800, 600)]
            [TestCase("\"M800x600\"", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("\"Maximized800x600\"", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("\"maximized800x600\"", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("\"m800x600\"", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("\"Minimized800x600\"", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("\"minimized800x600\"", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("\"N800x600\"", BrowserWindowState.Normal, 800, 600)]
            [TestCase("\"Normal80x600\"", BrowserWindowState.Normal, 800, 600)]
            [TestCase("\"normal80x600\"", BrowserWindowState.Normal, 800, 600)]

            [TestCase("'800x600'", BrowserWindowState.Normal, 800, 600)]
            [TestCase("'M800x600'", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("'Maximized800x600'", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("'maximized800x600'", BrowserWindowState.Maximized, 800, 600)]
            [TestCase("'m800x600'", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("'Minimized800x600'", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("'minimized800x600'", BrowserWindowState.Minimized, 800, 600)]
            [TestCase("'N800x600'", BrowserWindowState.Normal, 800, 600)]
            [TestCase("'Normal80x600'", BrowserWindowState.Normal, 800, 600)]
            [TestCase("'normal80x600'", BrowserWindowState.Normal, 800, 600)]

            public void geometry_specified(string option, BrowserWindowState expectedBrowserWindowState, int expectedWidth, int expectedHeight)
            {
                WindowGeometry windowGeometry = ArgOptions.ParseWindowGeometry(option);
                windowGeometry.ShouldNotBeNull();

                windowGeometry.State.ShouldEqual(expectedBrowserWindowState);
                windowGeometry.Size.Width = expectedWidth;
                windowGeometry.Size.Height = expectedHeight;
            }

        }
    }
}
