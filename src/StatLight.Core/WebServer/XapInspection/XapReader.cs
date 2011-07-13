
using System.Collections;
using System.IO.Packaging;
using System.Xml;
using Ionic.Zip;

namespace StatLight.Core.WebServer.XapInspection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Xml.Linq;
    using StatLight.Core.Common;

    public class XapReader
    {
        private readonly ILogger _logger;

        public XapReader(ILogger logger)
        {
            _logger = logger;
        }

        public TestFileCollection LoadXapUnderTest(string archiveFileName)
        {
            var files = new List<ITestFile>();
            string testAssemblyFullName = null;

            var fileStream = FileReader.ReadAllBytes(archiveFileName);

            using (IZipArchive archive = ZipArchiveFactory.Create(fileStream))
            {
                var appManifest = LoadAppManifest(archive);

                if (appManifest != null)
                {
                    string testAssemblyName = GetTestAssemblyNameFromAppManifest(appManifest);

                    AssemblyName assemblyName = GetAssemblyName(archive, testAssemblyName);
                    if (assemblyName != null)
                    {
                        testAssemblyFullName = assemblyName.ToString();
                    }
                }

                files.AddRange(archive.ToList());

                foreach (var item in files)
                    _logger.Debug("XapItems.FilesContainedWithinXap = {0}".FormatWith(item.FileName));
            }


            var xapItems = new TestFileCollection(_logger, testAssemblyFullName, files);


            return xapItems;
        }

        private static AssemblyName GetAssemblyName(IZipArchive zip1, string testAssemblyName)
        {
            string tempFileName = Path.GetTempFileName();
            var fileData = zip1.ReadFileIntoBytes(testAssemblyName);
            if (fileData != null)
            {
                File.WriteAllBytes(tempFileName, fileData);
                return AssemblyName.GetAssemblyName(tempFileName);
            }
            return null;
        }

        private static string GetTestAssemblyNameFromAppManifest(string appManifest)
        {
            var root = XElement.Parse(appManifest);

            var entryPointAssemblyNode = root.Attribute("EntryPointAssembly");

            if (entryPointAssemblyNode == null)
                throw new StatLightException("Cannot find the EntryPointAssembly attribute in the AppManifest.xaml");

            return entryPointAssemblyNode.Value + ".dll";
        }

        private static string LoadAppManifest(IZipArchive zip1)
        {
            var fileData = zip1.ReadFileIntoBytes("AppManifest.xaml");
            if (fileData != null)
            {
                string xaml = Encoding.UTF8.GetString(fileData);
                if (xaml[0] == '<')
                    return xaml;
                return xaml.Substring(1);
            }

            return null;
        }

        public static string GetRuntimeVersion(string xapPath)
        {
            using (var archive = ZipArchiveFactory.Read(xapPath))
            {
                var appManifestEntry = archive["AppManifest.xaml"];
                if (appManifestEntry == null)
                    return null;

                var xAppManifest = XElement.Load(appManifestEntry.ToStream());

                var runtimeVersion = xAppManifest.Attribute("RuntimeVersion");
                return runtimeVersion != null ? runtimeVersion.Value : null;
            }
        }
    }

    public static class FileReader
    {
        public static byte[] ReadAllBytes(string path)
        {
            var stopwatch = Stopwatch.StartNew();

            while (true)
            {
                try
                {
                    return File.ReadAllBytes(path);
                }
                catch (IOException ex)
                {
                    if (ex.Message.Contains("because it is being used by another process"))
                    {
                        Thread.Sleep(500);
                    }
                    else
                    {
                        throw;
                    }
                }

                // Don't wait on file forever... fail if it's locked for 15 seconds or more.
                if (stopwatch.Elapsed > TimeSpan.FromSeconds(15))
                {
                    throw new StatLightException("Could not seem read the file [{0}] as it appears to be locked by another process.".FormatWith(path));
                }

            }
        }
    }


    public static class ZipArchiveFactory
    {
        public static IZipArchive Create(byte[] zipBytes)
        {
            if (zipBytes == null) throw new ArgumentNullException("zipBytes");
            return new ZipArchive(zipBytes);
        }

        public static IZipArchive Read(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            var bytes = File.ReadAllBytes(filePath);
            return Create(bytes);
        }

        public static XElement GetAppManifest(this IZipArchive zipArchive)
        {
            if (zipArchive == null) throw new ArgumentNullException("zipArchive");
            byte[] appManifestEntry = zipArchive["AppManifest.xaml"];
            string stringFromByteArray = appManifestEntry.ToStringFromByteArray();
            Trace.WriteLine(stringFromByteArray);
            return XElement.Load(stringFromByteArray.ToStream());

        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface IZipArchive : IDisposable, IEnumerable<ITestFile>
    {
        byte[] this[string fileName] { get; }
        bool ContainsFile(string fileName);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        void AddFile(string fileName, byte[] value, bool replaceExisting = true);
        byte[] ToByteArray();
        byte[] ReadFileIntoBytes(string fileName);
    }


    //public class ZipArchive : IZipArchive, IEnumerable<ITestFile>
    //{
    //    private ZipStorer _zipFile;
    //    private MemoryStream _tempStream;

    //    public ZipArchive()
    //    {
    //        _tempStream = new MemoryStream();
    //        _zipFile = ZipStorer.Create(_tempStream, "test");
    //    }

    //    public ZipArchive(byte[] zipBytes)
    //    {
    //        _zipFile = ZipStorer.Open(zipBytes.ToStream(), FileAccess.Read);
    //    }

    //    public void Dispose()
    //    {
    //        _zipFile.Close();
    //    }

    //    public byte[] this[string fileName]
    //    {
    //        get
    //        {
    //            Trace.WriteLine("Looking for: " + fileName);
    //            foreach (var zipFile in _zipFile.Files)
    //            {
    //                Trace.WriteLine(zipFile.FilenameInZip);
    //                if (zipFile.FilenameInZip.Equals(fileName, StringComparison.OrdinalIgnoreCase))
    //                {
    //                    return ReadFileIntoBytes(fileName);
    //                }
    //            }

    //            throw new StatLightException("File not found within zip. [{0}]".FormatWith(fileName));
    //        }
    //    }

    //    public bool ContainsFile(string fileName)
    //    {
    //        Debug.Assert(_zipFile != null, "_zipFile != null");
    //        return _zipFile.Files.Any(zipFile => zipFile.FilenameInZip.ToString().Equals(fileName, StringComparison.OrdinalIgnoreCase));
    //    }

    //    public void AddFile(string fileName, byte[] value, bool replaceExisting = true)
    //    {
    //        if (replaceExisting)
    //        {
    //            if (ContainsFile(fileName))
    //            {
    //                List<ZipStorer.ZipFileEntry> fileToRemove = _zipFile.Files.Where(
    //                        zipFile =>
    //                        zipFile.FilenameInZip.ToString().Equals(fileName, StringComparison.OrdinalIgnoreCase)).
    //                        ToList();

    //                ZipStorer.RemoveEntries(ref _zipFile, fileToRemove);
    //            }
    //        }

    //        _zipFile.AddStream(ZipStorer.Compression.Store, fileName, value.ToStream(), DateTime.Now, "TEST");
    //    }

    //    public byte[] ToByteArray()
    //    {
    //        return _tempStream.ToArray();
    //    }

    //    private bool TryGetFile(string fileName, out ZipStorer.ZipFileEntry file)
    //    {
    //        foreach (var zipFileEntry in _zipFile.Files)
    //        {
    //            if (zipFileEntry.FilenameInZip.Equals(fileName, StringComparison.OrdinalIgnoreCase))
    //            {
    //                file = zipFileEntry;
    //                return true;
    //            }
    //        }

    //        file = null;
    //        return false;
    //    }

    //    public byte[] ReadFileIntoBytes(string fileName)
    //    {
    //        ZipStorer.ZipFileEntry file;
    //        if (!TryGetFile(fileName, out file))
    //            return null;

    //        using (var stream = new MemoryStream())
    //        {
    //            Trace.WriteLine(fileName);
    //            _zipFile.ExtractFile(file, stream);
    //            return stream.ToArray();
    //        }
    //    }

    //    public void AddFile(string fileName)
    //    {
    //        byte[] bytes = File.ReadAllBytes(fileName);
    //        AddFile(fileName, bytes, false);
    //    }

    //    //public void Save(string fileName)
    //    //{
    //    //    //ZipStorer zipStorer = ZipStorer.Create(fileName, "Test");
    //    //    _zipFile.Save(fileName);
    //    //}

    //    internal void SaveTemp()
    //    {
    //        _zipFile.Close();
    //        //var memoryStream = new MemoryStream();
    //        //_zipFile.Save(memoryStream);
    //    }

    //    public IEnumerator<ITestFile> GetEnumerator()
    //    {
    //        foreach (var file in _zipFile.Files)
    //        {
    //            yield return new TestFile(file.FilenameInZip, ReadFileIntoBytes(file.FilenameInZip));
    //        }
    //    }

    //    IEnumerator IEnumerable.GetEnumerator()
    //    {
    //        return GetEnumerator();
    //    }
    //}


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class ZipArchive : IZipArchive
    {
        private readonly ZipFile _zipFile;

        public ZipArchive()
        {
            _zipFile = new ZipFile();
        }

        public ZipArchive(byte[] value)
        {
            _zipFile = ZipFile.Read(value);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _zipFile.Dispose();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1065:DoNotRaiseExceptionsInUnexpectedLocations")]
        public byte[] this[string fileName]
        {
            get
            {
                foreach (var zipFile in _zipFile)
                {
                    if (zipFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase))
                    {
                        return ReadFileIntoBytes(fileName);
                    }
                }

                throw new StatLightException("File not found within zip. [{0}]".FormatWith(fileName));
            }
        }

        public bool ContainsFile(string fileName)
        {
            return _zipFile.Any(zipFile => zipFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase));
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public void AddFile(string fileName, byte[] value, bool replaceExisting = true)
        {
            if (replaceExisting)
            {
                if (ContainsFile(fileName))
                {
                    _zipFile.RemoveEntry(fileName);
                }
            }

            _zipFile.AddEntry(Path.GetFileName(fileName), Path.GetDirectoryName(fileName), value);
        }

        public byte[] ToByteArray()
        {
            using (var stream = new MemoryStream())
            {
                _zipFile.Save(stream);
                return stream.ToArray();
            }
        }

        public byte[] ReadFileIntoBytes(string fileName)
        {
            var file = _zipFile[fileName];
            if (file == null)
                return null;

            using (var stream = new MemoryStream())
            {
                Trace.WriteLine(fileName);
                file.Extract(stream);
                return stream.ToArray();
            }
        }

        public void AddFile(string fileName)
        {
            byte[] bytes = File.ReadAllBytes(fileName);
            AddFile(fileName, bytes, false);
        }

        public void Save(string fileName)
        {
            _zipFile.Save(fileName);
        }

        public IEnumerator<ITestFile> GetEnumerator()
        {
            foreach (var file in _zipFile)
            {
                yield return new TestFile(file.FileName, ReadFileIntoBytes(file.FileName));
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}

