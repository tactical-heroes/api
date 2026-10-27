using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking;

internal static class MatchmakingTestData
{
    internal static MatchmakingTimestamp At(int seconds)
    {
        return MatchmakingTimestamp.Create(
            new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero).AddSeconds(seconds));
    }

    internal static MatchSearch CreateSearch(Guid? playerId = null, string mode = "duel", int expiresAt = 300)
    {
        return MatchSearch.Create(
            PlayerId.Create(playerId ?? Guid.CreateVersion7()).Value,
            BuildId.Create(Guid.CreateVersion7()).Value,
            RatingSnapshot.Create(1500),
            MatchmakingMode.Create(mode).Value,
            At(0),
            At(expiresAt)).Value;
    }

    internal static Match CreateMatch()
    {
        return Match.Create(CreateSearch(), CreateSearch(), At(1), At(60)).Value;
    }
}
