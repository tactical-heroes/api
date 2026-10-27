using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildHeroes;
using PANiXiDA.TacticalHeroes.Builds.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Domain.Builds.Entities.BuildHeroes;

public sealed class BuildHeroTests
{
    [Fact(DisplayName = "Selected hero should preserve its references when created")]
    public void Create_Should_PreserveReferences_When_Created()
    {
        var buildId = BuildId.New();
        var definitionId = HeroId.New();

        var entry = BuildHero.Create(buildId, definitionId);

        entry.Id.Value.Version.ShouldBe(7);
        entry.BuildId.ShouldBe(buildId);
        entry.HeroId.ShouldBe(definitionId);
    }
}
