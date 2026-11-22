namespace PANiXiDA.TacticalHeroes.Builds.Application.Builds.Read.GetInternalBuild;

public sealed record BuildReadModel(Guid Id, Guid PlayerId) : IReadModel;
