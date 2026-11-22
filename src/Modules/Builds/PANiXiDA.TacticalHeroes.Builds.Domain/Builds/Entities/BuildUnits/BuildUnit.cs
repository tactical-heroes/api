using PANiXiDA.TacticalHeroes.Builds.Domain.Units;

namespace PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildUnits;

public sealed class BuildUnit : Entity<BuildUnitId>
{
    private BuildUnit(BuildUnitId id, BuildId buildId, UnitId unitId) : base(id)
    {
        BuildId = buildId;
        UnitId = unitId;
    }

    public BuildId BuildId { get; private set; }
    public UnitId UnitId { get; private set; }

    internal static BuildUnit Create(BuildId buildId, UnitId unitId)
    {
        return new BuildUnit(
            id: BuildUnitId.New(),
            buildId: buildId,
            unitId: unitId);
    }
}
