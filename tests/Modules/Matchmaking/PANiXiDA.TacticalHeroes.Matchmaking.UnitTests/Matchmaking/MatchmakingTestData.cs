using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Domain.Players;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.UnitTests.Matchmaking;

internal static class MatchmakingTestData
{
    internal static MatchmakingTimestamp At(int seconds)
    {
        return MatchmakingTimestamp.Create(
            new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero).AddSeconds(seconds));
    }

    internal static MatchSearch CreateSearch(Guid? playerId = null, MatchmakingMode? mode = null, int queuedAt = 0)
    {
        return MatchSearch.Create(
            PlayerId.Create(playerId ?? Guid.CreateVersion7()).Value,
            BuildId.Create(Guid.CreateVersion7()).Value,
            Rating.Create(1500),
            mode ?? MatchmakingMode.RankedDuel,
            At(queuedAt)).Value;
    }
}
