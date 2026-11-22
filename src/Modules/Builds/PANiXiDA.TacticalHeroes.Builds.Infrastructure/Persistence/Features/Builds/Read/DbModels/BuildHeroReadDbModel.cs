namespace PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Features.Builds.Read.DbModels;

public sealed class BuildHeroReadDbModel : ReadDbModel<Guid>
{
    public Guid BuildId { get; set; }
    public Guid HeroId { get; set; }

    public BuildReadDbModel? Build { get; set; }
}
