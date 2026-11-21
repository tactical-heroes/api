namespace PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;

public sealed record BuildReadModel(Guid Id, Guid PlayerId) : IReadModel
{
    public List<Guid> HeroIds { get; init; } = [];
    public List<Guid> UnitIds { get; init; } = [];
}
