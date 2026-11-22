using PANiXiDA.TacticalHeroes.Builds.Domain.Builds;
using PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildUnits;
using PANiXiDA.TacticalHeroes.Builds.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Builds.UnitTests.Domain.Builds.Entities.BuildUnits;

public sealed class BuildUnitTests
{
    [Fact(DisplayName = "Selected unit should preserve its references when created")]
    public void Create_Should_PreserveReferences_When_Created()
    {
        var buildId = BuildId.New();
        var definitionId = UnitId.New();

        var entry = BuildUnit.Create(buildId, definitionId);

        entry.Id.Value.Version.ShouldBe(7);
        entry.BuildId.ShouldBe(buildId);
        entry.UnitId.ShouldBe(definitionId);
    }
}
