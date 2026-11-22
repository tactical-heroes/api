namespace PANiXiDA.TacticalHeroes.Modules.Builds.Application.Builds.GetInternalBuild;

public sealed record GetInternalBuildQuery(Guid Id) : IQuery<Result<BuildReadModel>>;
