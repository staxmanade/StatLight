using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using StatLight.Core.Common;

namespace StatLight.Core.WebBrowser
{
    internal class OutOfBrowser : OutOfProcessWebBrowserBase
    {
        private readonly Uri _hostedXapUri;
        private readonly string _fileOnDiskPath;

        public OutOfBrowser(ILogger logger, Uri hostedXapUri, string fileOnDiskPath, bool forceBrowserStart, bool isStartingMultipleInstances) 
            : base(logger, hostedXapUri, forceBrowserStart, isStartingMultipleInstances)
        {
            _hostedXapUri = hostedXapUri;
            _fileOnDiskPath = fileOnDiskPath;

            logger.Debug("OutOfBrowser:_fileOnDiskPath={0}".FormatWith(_fileOnDiskPath));
            logger.Debug("OutOfBrowser:_hostedXapUri  ={0}".FormatWith(_hostedXapUri));
        }

        ~OutOfBrowser()
        {
            _logger.Debug("Disposing OutOfBrowser webBrowser");
        }

        protected override string GetCommandLineArguments(Uri uri)
        {
            // doesn't seem to matter the path- but the localhost:{port}/...xap path doesn't seem to work
            var path = "http://localhost:8887/GetHtmlTestPage?";
            //var path = uri.ToString();
            _logger.Debug("path:{0}".FormatWith(path));
            _logger.Debug("uri:{0}".FormatWith(uri.ToString()));
            return "/emulate:{0} /origin:{1} /debug".FormatWith(_fileOnDiskPath, path);
        }

        protected override string ProcessName
        {
            get { return "sllauncher"; }
        }

        protected override string ExePath
        {
            get
            {
                string pf = X86ProgramFilesFolder();
                return @"{0}\Microsoft Silverlight\sllauncher.exe".FormatWith(pf);
            }
        }
    }
}