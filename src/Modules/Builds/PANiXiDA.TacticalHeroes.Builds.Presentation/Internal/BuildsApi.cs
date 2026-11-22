using PANiXiDA.TacticalHeroes.Modules.Builds.Contracts.Builds;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Presentation.Internal;

internal sealed class BuildsApi(IMediator mediator) : IBuildsApi
{
    public async Task<BuildResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var result = await mediator.QueryAsync(BuildsApiMapper.ToQuery(id), cancellationToken);

        return result.IsSuccess ? BuildsApiMapper.ToResponse(result.Value) : null;
    }
}
