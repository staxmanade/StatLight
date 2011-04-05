using StatLight.Core.Configuration;

namespace StatLight.Core.Runners
{
    public interface IStatLightRunnerFactory
    {
        IRunner CreateContinuousTestRunner(StatLightConfiguration statLightConfiguration);
        IRunner CreateTeamCityRunner(StatLightConfiguration statLightConfiguration);
        IRunner CreateOnetimeConsoleRunner(StatLightConfiguration statLightConfiguration);
        IRunner CreateWebServerOnlyRunner(StatLightConfiguration statLightConfiguration);

        //[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        //IRunner CreateRemotelyHostedRunner(StatLightConfiguration statLightConfiguration);
    }
}