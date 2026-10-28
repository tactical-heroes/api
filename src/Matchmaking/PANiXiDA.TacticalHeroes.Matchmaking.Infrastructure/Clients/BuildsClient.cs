using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

public sealed class BuildsClient : IBuildsClient
{
    public Task<bool> IsOwnedByPlayerAsync(Guid buildId, Guid playerId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Build ownership is mocked until Builds is connected.
        return Task.FromResult(result: true);
    }
}
