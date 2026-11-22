using PANiXiDA.TacticalHeroes.Modules.Builds.Contracts.Builds;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Clients;

public sealed class BuildsClient(IBuildsApi buildsApi) : IBuildsClient
{
    public async Task<BuildReadModel?> GetByIdAsync(Guid buildId, CancellationToken cancellationToken)
    {
        var build = await buildsApi.GetByIdAsync(buildId, cancellationToken);

        return build is null ? null : new BuildReadModel(build.Id, build.PlayerId);
    }
}
