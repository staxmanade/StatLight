using StatLight.Core.Common;
using StatLight.Core.Events;
using StatLight.Core.Runners;
using StatLight.Core.WebServer;
using TinyIoC;

namespace StatLight.Core
{
    public static class BootStrapper
    {
        public static TinyIoCContainer Initialize(bool isRequestingDebug)
        {
            var ioc = new TinyIoCContainer();
            ioc.Register(ioc);
            ioc.Register(GetLogger(isRequestingDebug));
            ioc.Register<WebServerLocation>().AsSingleton();

            var eventAggregator = ioc.Resolve<EventAggregatorFactory>().Create();
            ioc.Register(eventAggregator);
            ioc.Register<IEventPublisher>(eventAggregator);
            ioc.Register<IEventSubscriptionManager>(eventAggregator);
            ioc.Register<IStatLightRunnerFactory, StatLightRunnerFactory>();

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