using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using NUnit.Framework;
using StatLight.Core.WebServer.XapInspection;

namespace StatLight.Core.Tests.WebServer.XapInspection
{
    [TestFixture]
    public class XapRewriterTests : FixtureBase
    {
        private XapRewriter _xapRewriter;
        private byte[] _originalXapHost;
        private byte[] _originalXapUnderTest;
        private byte[] _expectedXapHost;
        private string _expectedAppManifest;

        protected override void Before_all_tests()
        {
            base.Before_all_tests();

            _xapRewriter = new XapRewriter(base.TestLogger);


            var appManifest = @"<Deployment xmlns=""http://schemas.microsoft.com/client/2007/deployment"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" EntryPointAssembly=""StatLight.Client.Harness"" EntryPointType=""StatLight.Client.Harness.App"" RuntimeVersion=""4.0.50826.0"">
  <Deployment.Parts>
    <AssemblyPart x:Name=""StatLight.Client.Harness"" Source=""StatLight.Client.Harness.dll"" />
    <AssemblyPart x:Name=""System.Windows.Controls"" Source=""System.Windows.Controls.dll"" />
    <AssemblyPart x:Name=""System.Xml.Linq"" Source=""System.Xml.Linq.dll"" />
    <AssemblyPart x:Name=""System.Xml.Serialization"" Source=""System.Xml.Serialization.dll"" />
    <AssemblyPart x:Name=""Microsoft.Silverlight.Testing"" Source=""Microsoft.Silverlight.Testing.dll"" />
    <AssemblyPart x:Name=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight"" Source=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"" />
  </Deployment.Parts>
</Deployment>
";

            var originalXapHost = new ZipArchive();
            originalXapHost.AddTempFile("AppManifest.xaml", appManifest.ToByteArray());
            originalXapHost.AddTempFile("StatLight.Client.Harness.dll");
            originalXapHost.AddTempFile("StatLight.Client.Harness.MSTest.dll");
            originalXapHost.AddTempFile("System.Windows.Controls.dll");
            originalXapHost.AddTempFile("System.Xml.Linq.dll");
            originalXapHost.AddTempFile("System.Xml.Serialization.dll");
            originalXapHost.AddTempFile("Microsoft.Silverlight.Testing.dll");
            originalXapHost.AddTempFile("Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll");
            _originalXapHost = originalXapHost.ToByteArray();

            var appManifest2 = @"<Deployment xmlns=""http://schemas.microsoft.com/client/2007/deployment"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" EntryPointAssembly=""StatLight.IntegrationTests.Silverlight.MSTest"" EntryPointType=""StatLight.IntegrationTests.Silverlight.App"" RuntimeVersion=""4.0.50826.0"">
  <Deployment.Parts>
    <AssemblyPart x:Name=""StatLight.IntegrationTests.Silverlight.MSTest"" Source=""StatLight.IntegrationTests.Silverlight.MSTest.dll"" />
    <AssemblyPart x:Name=""Microsoft.Silverlight.Testing"" Source=""Microsoft.Silverlight.Testing.dll"" />
    <AssemblyPart x:Name=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight"" Source=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"" />
  </Deployment.Parts>
</Deployment>
";

            var originalXapUnderTest = new ZipArchive();
            originalXapUnderTest.AddTempFile("/AppManifest.xaml", appManifest2.ToByteArray());
            originalXapUnderTest.AddTempFile("/StatLight.IntegrationTests.Silverlight.MSTest.dll");
            originalXapUnderTest.AddTempFile("/Microsoft.Silverlight.Testing.dll");
            originalXapUnderTest.AddTempFile("/Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll");
            originalXapUnderTest.AddTempFile("/Test/Test/Test.xml", "Hello".ToByteArray());

            // This was a crazy case that the Sterling db project exposed to StatLight (They had duplicate test assemblies)
            originalXapUnderTest.AddTempFile("/Binaries/Microsoft.Silverlight.Testing.dll", new byte[] { 1, 2 });
            originalXapUnderTest.AddTempFile("/Binaries/Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll", new byte[] { 1, 2 });

            _originalXapUnderTest = originalXapUnderTest.ToByteArray();

            _expectedAppManifest = @"<Deployment xmlns=""http://schemas.microsoft.com/client/2007/deployment"" xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"" EntryPointAssembly=""StatLight.Client.Harness"" EntryPointType=""StatLight.Client.Harness.App"" RuntimeVersion=""4.0.50826.0"">
  <Deployment.Parts>
    <AssemblyPart x:Name=""StatLight.Client.Harness"" Source=""StatLight.Client.Harness.dll"" />
    <AssemblyPart x:Name=""System.Windows.Controls"" Source=""System.Windows.Controls.dll"" />
    <AssemblyPart x:Name=""System.Xml.Linq"" Source=""System.Xml.Linq.dll"" />
    <AssemblyPart x:Name=""System.Xml.Serialization"" Source=""System.Xml.Serialization.dll"" />
    <AssemblyPart x:Name=""Microsoft.Silverlight.Testing"" Source=""Microsoft.Silverlight.Testing.dll"" />
    <AssemblyPart x:Name=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight"" Source=""Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll"" />
    <AssemblyPart x:Name=""StatLight.IntegrationTests.Silverlight.MSTest"" Source=""StatLight.IntegrationTests.Silverlight.MSTest.dll"" />
  </Deployment.Parts>
</Deployment>
";
            var expectedXapHost = new ZipArchive();
            expectedXapHost.AddTempFile("/StatLight.Client.Harness.dll");
            expectedXapHost.AddTempFile("/StatLight.Client.Harness.MSTest.dll");
            expectedXapHost.AddTempFile("/System.Windows.Controls.dll");
            expectedXapHost.AddTempFile("/System.Xml.Linq.dll");
            expectedXapHost.AddTempFile("/System.Xml.Serialization.dll");
            expectedXapHost.AddTempFile("/Microsoft.Silverlight.Testing.dll");
            expectedXapHost.AddTempFile("/Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll");
            expectedXapHost.AddTempFile("/StatLight.IntegrationTests.Silverlight.MSTest.dll");
            expectedXapHost.AddTempFile("/Test/Test/Test.xml", "Hello".ToByteArray());
            expectedXapHost.AddTempFile("/AppManifest.xaml", _expectedAppManifest.ToByteArray());

            // This was a crazy case that the Sterling db project exposed to StatLight (They had duplicate test assemblies)
            expectedXapHost.AddTempFile("/Binaries/Microsoft.Silverlight.Testing.dll");
            expectedXapHost.AddTempFile("/Binaries/Microsoft.VisualStudio.QualityTools.UnitTesting.Silverlight.dll");

            _expectedXapHost = expectedXapHost.ToByteArray();
        }

        [Test]
        public void Should_serialize_zip_bytes_correctly()
        {
            _originalXapHost.ShouldEqual(_originalXapHost);
        }

        [Test]
        public void Should_be_able_to_load_created_zip()
        {

            //var someFileToStore = new MemoryStream(Encoding.UTF8.GetBytes("Hello World!")); ;
            //var zipStream = new MemoryStream();
            //ZipStorer zip = ZipStorer.Create(zipStream, "test");
            ////zip.EncodeUTF8 = true;
            ////zip.AddStream(ZipStorer.Compression.Store, "/Hello.txt", someFileToStore, DateTime.Now.AddDays(-1), "Test");
            //byte[] bytes = zipStream.ToArray();

            //zip.Close();

            ////Encoding.UTF8.GetString(bytes).ShouldEqual("Hello World!");
            //Trace.WriteLine(bytes.Length);
            //var fileAsStream = new MemoryStream(bytes);

            //var tempFileName = Path.GetTempFileName();
            //File.WriteAllBytes(tempFileName, bytes);
            //ZipStorer.Open(tempFileName, FileAccess.Read);
            // Throws exception here...
            //ZipStorer.Open(fileAsStream, FileAccess.Read);

            //ZipStorer.Open(@"C:\Code\StatLight\src\build\bin\Debug\StatLight.Client.For.April2010.xap", FileAccess.Read);
        }


        private static void AssertZipsEqual(IZipArchive expected, IZipArchive actual)
        {
            actual.Count().ShouldEqual(expected.Count(), "zip files contain different counts");

            var allFiles = actual.Select(s => s.FileName).ToArray();

            foreach (var expectedFile in expected)
            {
                string firstOrDefault = allFiles.Where(w => w.Equals(expectedFile.FileName, StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                firstOrDefault.ShouldNotBeNull("Could not find file {0} in files \n{1}".FormatWith(
                    expectedFile.FileName,
                    string.Join(Environment.NewLine, allFiles)));
                //allFiles.Contains(expectedFile.FileName).ShouldBeTrue();
            }
            //for (int i = 0; i < expected.Count; i++)
            //{
            //    var expectedFile = expected[i];
            //    var actualFile = actual[i];
            //
            //    actualFile.FileName.ShouldEqual(expectedFile.FileName);
            //    //actualFile.ToByteArray().ShouldEqual(expectedFile.ToByteArray(), "File [{0}] bytes not same.".FormatWith(actualFile.FileName));
            //}

        }

        private class TempTestFile : ITestFile
        {
            private readonly string _fileName;

            public TempTestFile(string fileName)
            {
                _fileName = fileName;
            }

            public string FileName
            {
                get { return _fileName; }
            }

            public byte[] File
            {
                get { return new byte[] { 1, 2, 3 }; }
            }
        }
    }



    public static class Extensions
    {
        public static void AddTempFile(this ZipArchive zipArchive, string fileName, byte[] value = null)
        {
            if (value == null)
                value = new byte[] { 1, 2 };

            XapRewriter.AddFileInternal(zipArchive, fileName, value);
        }
    }
}