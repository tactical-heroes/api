namespace PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;

public sealed record BuildReadModel(Guid Id, Guid PlayerId, List<Guid> HeroIds, List<Guid> UnitIds) : IReadModel;
