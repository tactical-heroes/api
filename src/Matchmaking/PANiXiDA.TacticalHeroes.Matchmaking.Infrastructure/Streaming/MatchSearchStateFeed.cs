using System.Runtime.CompilerServices;

using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming;

internal sealed class MatchSearchStateFeed(
    MatchSearchConnections connections,
    IServiceScopeFactory scopeFactory) : IMatchSearchStateFeed
{
    public async Task<IMatchSearchStateSubscription> SubscribeAsync(Guid playerId, CancellationToken cancellationToken)
    {
        var connection = connections.Subscribe(playerId);
        try
        {
            var state = await ReadStateAsync(
                scopeFactory: scopeFactory, playerId: playerId, cancellationToken: cancellationToken);
            return new Subscription(
                connections: connections, connection: connection, scopeFactory: scopeFactory, current: state);
        }
        catch
        {
            connections.Remove(connection);
            throw;
        }
    }

    private static async Task<MatchSearchStateReadModel> ReadStateAsync(
        IServiceScopeFactory scopeFactory,
        Guid playerId,
        CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var state = await scope.ServiceProvider.GetRequiredService<IMatchSearchesReadRepository>()
            .GetStateByPlayerIdAsync(playerId, cancellationToken);

        return state
            ?? new MatchSearchStateReadModel(
                Version: 0,
                MatchSearchId: null,
                Status: null,
                GameId: null,
                QueuedAt: null,
                ExpiresAt: null);
    }

    private sealed class Subscription(
        MatchSearchConnections connections,
        MatchSearchConnections.Connection connection,
        IServiceScopeFactory scopeFactory,
        MatchSearchStateReadModel current) : IMatchSearchStateSubscription
    {
        public MatchSearchStateReadModel Current { get; } = current;

        public async IAsyncEnumerable<MatchSearchStateReadModel> ReadChangesAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var version = Current.Version;
            await foreach (var _ in connection.Changes.Reader.ReadAllAsync(cancellationToken))
            {
                var state = await ReadStateAsync(
                    scopeFactory: scopeFactory, playerId: connection.PlayerId, cancellationToken: cancellationToken);
                if (state.Version > version)
                {
                    version = state.Version;
                    yield return state;
                }
            }
        }

        public ValueTask DisposeAsync()
        {
            connections.Remove(connection);
            return ValueTask.CompletedTask;
        }
    }
}
