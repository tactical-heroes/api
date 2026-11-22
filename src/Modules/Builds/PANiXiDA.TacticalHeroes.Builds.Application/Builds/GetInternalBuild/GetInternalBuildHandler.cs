using PANiXiDA.TacticalHeroes.Builds.Application.Builds.Abstractions;

namespace PANiXiDA.TacticalHeroes.Builds.Application.Builds.GetInternalBuild;

public sealed class GetInternalBuildHandler(IBuildsReadRepository buildsReadRepository)
    : IQueryHandler<GetInternalBuildQuery, Result<BuildReadModel>>
{
    public async Task<Result<BuildReadModel>> HandleAsync(
        GetInternalBuildQuery query,
        CancellationToken cancellationToken)
    {
        var build = await buildsReadRepository.GetByIdAsync(query.Id, cancellationToken);

        return build is null
            ? Result.Failure<BuildReadModel>(Error.NotFound("Build was not found."))
            : Result.Success(build);
    }
}
