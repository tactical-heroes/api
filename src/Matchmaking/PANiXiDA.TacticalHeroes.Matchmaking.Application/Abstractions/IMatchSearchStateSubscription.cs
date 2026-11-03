using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IMatchSearchStateSubscription : IAsyncDisposable
{
    MatchSearchStateReadModel Current { get; }

    IAsyncEnumerable<MatchSearchStateReadModel> ReadChangesAsync(CancellationToken cancellationToken);
}
