namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read.DbModels;

public sealed class BuildUnitReadDbModel : ReadDbModel<Guid>
{
    public Guid BuildId { get; set; }
    public Guid UnitId { get; set; }

    public BuildReadDbModel? Build { get; set; }
}
