using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.BuildSnapshots;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking;

internal static class MatchmakingTestData
{
    internal static MatchmakingTimestamp At(int seconds)
    {
        return MatchmakingTimestamp.Create(
            new DateTimeOffset(2026, 10, 26, 8, 0, 0, TimeSpan.Zero).AddSeconds(seconds));
    }

    internal static MatchmakingTicket Ticket(Guid? playerId = null, string mode = "duel")
    {
        return MatchmakingTicket.Create(
            PlayerId.Create(playerId ?? Guid.CreateVersion7()).Value,
            BuildSnapshotId.Create(Guid.CreateVersion7()).Value,
            RatingSnapshot.Create(1500),
            MatchmakingMode.Create(mode).Value,
            At(0),
            At(300)).Value;
    }

    internal static IUnitOfWork UnitOfWork(IServiceProvider services)
    {
        return services.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext));
    }

    internal static async Task<MatchAssignment> SaveAssignmentAsync(
        IServiceProvider services,
        CancellationToken cancellationToken)
    {
        var first = Ticket();
        var second = Ticket();
        var assignment = MatchAssignment.Create(first, second, At(1), At(60)).Value;
        first.Reserve(assignment.Id, At(1)).IsSuccess.ShouldBeTrue();
        second.Reserve(assignment.Id, At(1)).IsSuccess.ShouldBeTrue();
        var tickets = services.GetRequiredService<IMatchmakingTicketsRepository>();
        tickets.Add(first);
        tickets.Add(second);
        services.GetRequiredService<IMatchAssignmentsRepository>().Add(assignment);
        await UnitOfWork(services).SaveChangesAsync(cancellationToken);

        return assignment;
    }
}
