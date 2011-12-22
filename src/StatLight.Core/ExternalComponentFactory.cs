using System;
using System.ComponentModel.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using StatLight.Core.Common;
using StatLight.Core.Events;
using StatLight.Core.Runners;
using StatLight.Core.WebBrowser;

namespace StatLight.Core
{
    public class ExternalComponentFactory
    {
        private readonly ILogger _logger;

        public ExternalComponentFactory(ILogger logger)
        {
            _logger = logger;
        }

        private static string GetFullPath(string path)
        {
            if (!Path.IsPathRooted(path) && AppDomain.CurrentDomain.BaseDirectory != null)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            return Path.GetFullPath(path);
        }

        public IWebBrowser CreatePhone(Func<byte[]> hostXap)
        {
            var fileName = AppDomain.CurrentDomain.BaseDirectory + @"\StatLight.WindowsPhoneEmulator.dll";
            var assembly = Assembly.LoadFrom(fileName);
            var assemblyCatalog = new AssemblyCatalog(assembly);
            using (var compositionContainer = new CompositionContainer(assemblyCatalog))
            {
                var phoneEmulator = compositionContainer.GetExport<IPhoneEmulator>();
                var phoneEmulatorWrapper = phoneEmulator.Value;
                return phoneEmulatorWrapper.Create(logger: _logger, hostXap: hostXap);
            }
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void LoadUpExtensionsForTestingReportEvents(IEventSubscriptionManager eventSubscriptionManager)
        {
            if (eventSubscriptionManager == null) throw new ArgumentNullException("eventSubscriptionManager");
            try
            {
                var path = GetFullPath("Extensions");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                using (var directoryCatalog = new DirectoryCatalog(path))
                using (var compositionContainer = new CompositionContainer(directoryCatalog))
                {

                    var extensions = compositionContainer.GetExports<ITestingReportEvents>().ToList();
                    if (extensions.Any())
                    {
                        _logger.Debug("********** Extensions **********");
                        foreach (var lazyExtension in extensions)
                        {
                            var extensionInstance = lazyExtension.Value;
                            _logger.Debug("* Adding - {0}".FormatWith(extensionInstance.GetType().FullName));
                            eventSubscriptionManager.AddListener(extensionInstance);
                        }
                        _logger.Debug("********************************");
                    }
                }
            }
            catch (ReflectionTypeLoadException rfex)
            {
                string loaderExceptionMessages = "";
                foreach (var t in rfex.LoaderExceptions)
                {
                    loaderExceptionMessages += "   -  ";
                    loaderExceptionMessages += t.Message;
                    loaderExceptionMessages += Environment.NewLine;
                }

                string msg = @"
********************* ReflectionTypeLoadException *********************
***** Begin Loader Exception Messages *****
{0}
***** End Loader Exception Messages *****
".FormatWith(loaderExceptionMessages);

                _logger.Error(msg);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to initialize extension. Error:{0}{1}".FormatWith(Environment.NewLine, e.ToString()));
            }
        }
    }
}