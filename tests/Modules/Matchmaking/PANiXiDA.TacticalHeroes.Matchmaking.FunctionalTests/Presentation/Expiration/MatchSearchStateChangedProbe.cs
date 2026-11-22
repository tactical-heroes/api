using System.Collections.Concurrent;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration;

public sealed class MatchSearchStateChangedProbe
{
    private readonly ConcurrentQueue<MatchSearchStateChanged> _events = new();

    public IReadOnlyList<MatchSearchStateChanged> Events => [.. _events];

    public void Record(MatchSearchStateChanged domainEvent)
    {
        _events.Enqueue(domainEvent);
    }
}
