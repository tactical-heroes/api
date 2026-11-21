using PANiXiDA.TacticalHeroes.Battle.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Application.ReadModels;
using PANiXiDA.TacticalHeroes.Builds.Contracts.Builds;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Clients;

public sealed class BuildsClient(IBuildsApi buildsApi) : IBuildsClient
{
    public async Task<BuildReadModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var build = await buildsApi.GetByIdAsync(id, cancellationToken);

        return build is null ? null : new BuildReadModel(
            Id: build.Id,
            PlayerId: build.PlayerId,
            HeroIds: build.HeroIds,
            UnitIds: build.UnitIds);
    }
}
