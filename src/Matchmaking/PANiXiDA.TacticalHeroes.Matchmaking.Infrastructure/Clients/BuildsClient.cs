using PANiXiDA.TacticalHeroes.Builds.Contracts.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

public sealed class BuildsClient(IBuildsApi buildsApi) : IBuildsClient
{
    public async Task<BuildReadModel?> GetByIdAsync(Guid buildId, CancellationToken cancellationToken)
    {
        var build = await buildsApi.GetByIdAsync(buildId, cancellationToken);

        return build is null ? null : new BuildReadModel(build.Id, build.PlayerId);
    }
}
