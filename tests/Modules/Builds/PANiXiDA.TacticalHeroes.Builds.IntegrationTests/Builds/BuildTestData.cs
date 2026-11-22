using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.ValueObjects;
using PANiXiDA.TacticalHeroes.Builds.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Builds.IntegrationTests.Builds;

internal static class BuildTestData
{
    internal static Build Create()
    {
        return Build.Create(PlayerId.New(), BuildName.Create("Northern army").Value);
    }
}
