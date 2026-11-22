namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Features.Builds.Read.DbModels;

public sealed class BuildReadDbModel : AuditableReadDbModel<Guid>
{
    public Guid PlayerId { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<BuildHeroReadDbModel> Heroes { get; set; } = [];
    public ICollection<BuildUnitReadDbModel> Units { get; set; } = [];
}
