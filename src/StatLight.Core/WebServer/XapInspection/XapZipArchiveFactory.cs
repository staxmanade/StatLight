using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Ionic.Zip;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapInspection
{
    public static class XapZipArchiveFactory
    {
        public static IXapZipArchive Create(byte[] zipBytes)
        {
            if (zipBytes == null) throw new ArgumentNullException("zipBytes");
            return new XapZipArchive(zipBytes);
        }

        public static IXapZipArchive Read(string filePath)
        {
            if (filePath == null) throw new ArgumentNullException("filePath");
            var bytes = File.ReadAllBytes(filePath);
            return Create(bytes);
        }

        public static XElement GetAppManifest(this IXapZipArchive xapZipArchive)
        {
            if (xapZipArchive == null) throw new ArgumentNullException("xapZipArchive");
            byte[] appManifestEntry = xapZipArchive["AppManifest.xaml"];
            string stringFromByteArray = appManifestEntry.ToStringFromByteArray();
            return XElement.Load(stringFromByteArray.ToStream());
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public interface IXapZipArchive : IDisposable, IEnumerable<ITestFile>
    {
        byte[] this[string fileName] { get; }
        bool ContainsFile(string fileName);
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        void AddFile(string fileName, byte[] value, bool replaceExisting = true);
        byte[] ToByteArray();
        byte[] ReadFileIntoBytes(string fileName);
    }


    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix")]
    public class XapZipArchive : IXapZipArchive
    {
        private readonly ZipFile _zipFile;

        public XapZipArchive()
        {
            _zipFile = new ZipFile();
        }

        public XapZipArchive(byte[] value)
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
                if (_zipFile.Any(zipFile => zipFile.FileName.Equals(fileName, StringComparison.OrdinalIgnoreCase)))
                {
                    return ReadFileIntoBytes(fileName);
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