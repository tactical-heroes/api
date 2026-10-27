using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
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

    internal static MatchSearch CreateSearch(Guid? playerId = null, MatchmakingMode? mode = null, int expiresAt = 300)
    {
        return MatchSearch.Create(
            PlayerId.Create(playerId ?? Guid.CreateVersion7()).Value,
            BuildId.Create(Guid.CreateVersion7()).Value,
            Raiting.Create(1500),
            mode ?? MatchmakingMode.RankedDuel,
            At(0),
            At(expiresAt)).Value;
    }
}
