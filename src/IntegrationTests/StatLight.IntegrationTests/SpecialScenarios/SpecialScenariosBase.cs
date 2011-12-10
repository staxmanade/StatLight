using StatLight.IntegrationTests.ProviderTests;

namespace StatLight.IntegrationTests.SpecialScenarios
{
    public abstract class SpecialScenariosBase : IntegrationFixtureBase
    {
        protected override string GetTestXapPath()
        {
            return TestXapFileLocations.SilverlightIntegrationTests;
        }
    }
}