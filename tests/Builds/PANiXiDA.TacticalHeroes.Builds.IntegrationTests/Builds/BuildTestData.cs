using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.Builds.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Builds;

internal static class BuildTestData
{
    internal static Build Create()
    {
        return Build.Create(PlayerId.New(), BuildName.Create("Northern army").Value);
    }
}
