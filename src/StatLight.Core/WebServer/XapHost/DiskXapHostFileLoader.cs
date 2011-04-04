using System;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapHost
{
    using System.IO;

    public class DiskXapHostFileLoader : IXapHostFileLoader
    {
        private readonly ILogger _logger;
        private readonly string _fileName;

        public DiskXapHostFileLoader(ILogger logger, string baseDirectory, string fileName)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            if (fileName == null) throw new ArgumentNullException("fileName");

            _logger = logger;
            var pathToThisExe = baseDirectory;
            _fileName = Path.Combine(pathToThisExe, fileName);

            if (!File.Exists(_fileName))
                logger.Debug("DiskXapHostFileLoader cannot find file - {0}".FormatWith(_fileName));
        }

        public byte[] LoadXapHost()
        {
            _logger.Debug("Loading XapHost [" + _fileName + "]");

            var fileInfo = new FileInfo(_fileName);
            var file = fileInfo.OpenRead();
            var stuff = new byte[file.Length];
            file.Read(stuff, 0, (int)file.Length);
            return stuff;
        }
    }
}