using System.Collections.Concurrent;

using PANiXiDA.TacticalHeroes.Matchmaking.Contracts.MatchSearches;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

public sealed class MatchFoundProbe
{
    private readonly ConcurrentQueue<MatchFound> _events = new();

    public IReadOnlyList<MatchFound> Events => _events.ToArray();

    public void Record(MatchFound matchFound)
    {
        _events.Enqueue(matchFound);
    }
}
