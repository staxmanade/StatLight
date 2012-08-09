using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using StatLight.Core.Common;
using StatLight.Core.Configuration;
using StatLight.Core.Events;
using TinyIoC;

namespace StatLight.Core.Runners
{
    public class ExtensionResolver
    {
        private readonly ILogger _logger;
        private readonly IEventSubscriptionManager _eventSubscriptionManager;

        public ExtensionResolver(ILogger logger, IEventSubscriptionManager eventSubscriptionManager)
        {
            _logger = logger;
            _eventSubscriptionManager = eventSubscriptionManager;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        public void AddExtensionsToEventAggregator(TinyIoCContainer ioc)
        {
            var extensions = GetExtensions(ioc).ToList();

            if (!extensions.Any())
                return;

            _logger.Debug("********** Extensions **********");
            foreach (var extensionInstance in extensions)
            {
                _logger.Debug("* Adding - {0}".FormatWith(extensionInstance.GetType().FullName));
                _eventSubscriptionManager.AddListener(extensionInstance);
            }
            _logger.Debug("********************************");
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        private IEnumerable<object> GetExtensions(TinyIoCContainer ioc)
        {
            var testingReportEventsExtensions = new List<IShouldBeAddedToEventAggregator>();
            try
            {
                var path = GetFullPath("Extensions");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                var files = Directory.GetFiles(path, "*.dll");

                var inputOptions = ioc.Resolve<InputOptions>();
                files = files.Union(inputOptions.ExtensionDllPaths).ToArray();
                if(files.Length == 0)
                {
                    return Enumerable.Empty<object>();
                }
                var assemblies = files.Select(TryLoadAssembly).Where(a => a != null).ToArray();
                var types = assemblies.SelectMany(a => a.SafeGetTypes()).ToArray();
                var extensionTypes = types
                    .Where(typeof(IShouldBeAddedToEventAggregator).IsAssignableFrom)
                    .Where(t=>t.IsClass && t.IsAbstract == false)
                    .ToArray();
                foreach (var type in extensionTypes)
                {
                    ioc.Register(type, type.FullName);
                }
                foreach (var type in extensionTypes)
                {
                    testingReportEventsExtensions.Add((IShouldBeAddedToEventAggregator) ioc.Resolve(type, type.FullName));
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

                string msg =
                    @"
********************* ReflectionTypeLoadException *********************
***** Begin Loader Exception Messages *****
{0}
***** End Loader Exception Messages *****
"
                        .FormatWith(loaderExceptionMessages);

                _logger.Error(msg);
            }
            catch (Exception e)
            {
                _logger.Error("Failed to initialize extension. Error:{0}{1}".FormatWith(Environment.NewLine, e.ToString()));
            }

            return testingReportEventsExtensions;
        }

        private static string GetFullPath(string path)
        {
            if (!Path.IsPathRooted(path) && AppDomain.CurrentDomain.BaseDirectory != null)
            {
                path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
            }

            return Path.GetFullPath(path);
        }

        private Assembly TryLoadAssembly(string path)
        {
            var exception = default(Exception);
            try
            {
                AssemblyName name;
                try
                {
                    name = AssemblyName.GetAssemblyName(path);
                }
                catch (ArgumentException)
                {
                    name = new AssemblyName {CodeBase = path};
                }
                return Assembly.Load(name);
            }
            catch (FileNotFoundException e)
            {
                exception = e;
            }
            catch (FileLoadException e)
            {
                exception = e;
            }
            catch (BadImageFormatException e)
            {
                exception = e;
            }
            catch (ReflectionTypeLoadException e)
            {
                exception = e;
            }

            string msg =
                @"
********************* Error loading assembly *********************
{1}
***** Begin Loader Exception Messages *****
{0}
***** End Loader Exception Messages *****
"
                    .FormatWith(exception.Message, path);

            _logger.Error(msg);
            return null;
        }
    }
}