using System;
using System.IO;
using StatLight.Core.Common;

namespace StatLight.Core.WebServer.XapInspection
{
    public class AssemblyResolverFactory
    {
        private readonly ILogger _logger;

        public AssemblyResolverFactory(ILogger logger)
        {
            _logger = logger;
        }

        public AssemblyResolverBase Create(bool isPhoneRun, DirectoryInfo assembliesOriginalDirectory)
        {
            if (isPhoneRun)
                return new PhoneAssemblyResolver(_logger, assembliesOriginalDirectory);

            return new SilverlightAssemblyResolver(_logger, assembliesOriginalDirectory);
        }
    }
}