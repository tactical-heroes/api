using PANiXiDA.TacticalHeroes.Builds.Domain.Heroes;

namespace PANiXiDA.TacticalHeroes.Builds.Domain.Builds.Entities.BuildHeroes;

public sealed class BuildHero : Entity<BuildHeroId>
{
    private BuildHero(BuildHeroId id, BuildId buildId, HeroId heroId) : base(id)
    {
        BuildId = buildId;
        HeroId = heroId;
    }

    public BuildId BuildId { get; private set; }
    public HeroId HeroId { get; private set; }

    internal static BuildHero Create(BuildId buildId, HeroId heroId)
    {
        return new BuildHero(
            id: BuildHeroId.New(),
            buildId: buildId,
            heroId: heroId);
    }
}
