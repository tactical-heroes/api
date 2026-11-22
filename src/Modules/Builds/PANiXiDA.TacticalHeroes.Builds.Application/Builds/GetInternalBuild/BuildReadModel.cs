namespace PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;

public sealed record BuildReadModel(Guid Id, Guid PlayerId) : IReadModel;
