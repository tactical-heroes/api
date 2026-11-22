namespace PANiXiDA.TacticalHeroes.Builds.Application.Builds.Read.GetInternalBuild;

public sealed record GetInternalBuildQuery(Guid Id) : IQuery<Result<BuildReadModel>>;
