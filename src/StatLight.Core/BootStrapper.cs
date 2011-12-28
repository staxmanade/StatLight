using System;
using StatLight.Core.Common;
using StatLight.Core.Common.Logging;
using StatLight.Core.Configuration;
using StatLight.Core.Events;
using StatLight.Core.Runners;
using StatLight.Core.WebServer;
using TinyIoC;

namespace StatLight.Core
{
    public static class BootStrapper
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed"), System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static TinyIoCContainer Initialize(InputOptions inputOptions,
            ILogger overrideLogger = null)
        {
            if (inputOptions == null) throw new ArgumentNullException("inputOptions");
            var ioc = new TinyIoCContainer();
            ILogger logger = overrideLogger ?? GetLogger(inputOptions.IsRequestingDebug);
            ioc.Register(logger);

            ioc.Resolve<SettingsOverrideApplicator>()
                .ApplySettingsFrom(inputOptions.SettingsOverride, Properties.Settings.Default);

            inputOptions.DumpValuesForDebug(logger);
            ioc.Register(ioc);
            ioc.Register(inputOptions);
            ioc.Register<WebServerLocation>().AsSingleton();
            ioc.Register<IStatLightRunnerFactory, StatLightRunnerFactory>();

            var eventAggregator = ioc.Resolve<EventAggregatorFactory>().Create();
            ioc.Register(eventAggregator);
            ioc.Register<IEventPublisher>(eventAggregator);
            ioc.Register<IEventSubscriptionManager>(eventAggregator);


            ioc.Register<ResponseFactory>().AsSingleton();

            ioc.Register<IPostHandler, PostHandler>().AsSingleton();
            ioc.Register<ICurrentStatLightConfiguration, CurrentStatLightConfiguration>();

            return ioc;
        }


        private static ILogger GetLogger(bool isRequestingDebug)
        {
            ILogger logger;
            if (isRequestingDebug)
            {
                logger = new ConsoleLogger(LogChatterLevels.Full);
            }
            else
            {
#if DEBUG
                logger = new ConsoleLogger(LogChatterLevels.Full);
#else
                logger = new ConsoleLogger(LogChatterLevels.Error | LogChatterLevels.Warning | LogChatterLevels.Information);
#endif
            }
            return logger;
        }
    }
}