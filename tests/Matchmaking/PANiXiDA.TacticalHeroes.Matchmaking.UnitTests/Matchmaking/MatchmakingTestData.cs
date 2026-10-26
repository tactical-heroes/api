using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking;

internal static class MatchmakingTestData
{
    internal static MatchmakingTimestamp At(int seconds)
    {
        return MatchmakingTimestamp.Create(
            new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero).AddSeconds(seconds));
    }

    internal static MatchmakingTicket Ticket(Guid? playerId = null, string mode = "duel", int expiresAt = 300)
    {
        return MatchmakingTicket.Create(
            PlayerReference.Create(playerId ?? Guid.CreateVersion7()).Value,
            BuildSnapshotReference.Create(Guid.CreateVersion7()).Value,
            RatingSnapshot.Create(1500),
            MatchmakingMode.Create(mode).Value,
            At(0),
            At(expiresAt)).Value;
    }

    internal static MatchAssignment Assignment()
    {
        return MatchAssignment.Create(Ticket(), Ticket(), At(1), At(60)).Value;
    }
}
