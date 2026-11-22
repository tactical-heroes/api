namespace PANiXiDA.TacticalHeroes.Builds.Contracts.Builds;

public sealed record BuildResponse(Guid Id, Guid PlayerId)
{
    public List<Guid> HeroIds { get; init; } = [];
    public List<Guid> UnitIds { get; init; } = [];
}
