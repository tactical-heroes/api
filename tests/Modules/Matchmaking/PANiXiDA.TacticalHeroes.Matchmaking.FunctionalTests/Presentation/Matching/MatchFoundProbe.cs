using System.Collections.Concurrent;

using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Contracts.MatchSearches;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.FunctionalTests.Presentation.Matching;

public sealed class MatchFoundProbe
{
    private readonly ConcurrentQueue<MatchFound> _events = new();

    public IReadOnlyList<MatchFound> Events => [.. _events];

    public void Record(MatchFound matchFound)
    {
        _events.Enqueue(matchFound);
    }
}
