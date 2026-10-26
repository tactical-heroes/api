using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchAssignments.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchAssignments.Read.DbModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read.DbModels;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchAssignments.Write;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchAssignmentsRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Repository should preserve pair and game when reloaded between transitions")]
    public async Task GetByIdAsync_Should_PreservePairAndGame_When_ReloadedBetweenTransitions()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Domain.MatchAssignments.MatchAssignment assignment;
        await using (var scope = fixture.CreateScope())
        {
            assignment = await SaveAssignmentAsync(scope.ServiceProvider, cancellationToken);
        }

        var game = GameReference.Create(Guid.CreateVersion7()).Value;
        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
            var restored = (await repository.GetByIdAsync(assignment.Id, cancellationToken))!;
            restored.Status.ShouldBe(MatchAssignmentStatus.CreatingGame);
            restored.MatchedAt.ShouldBe(At(1));
            restored.CreationDeadline.ShouldBe(At(60));
            restored.Participants.Count.ShouldBe(2);
            restored.GetDomainEvents().OfType<GameCreationRequested>().ShouldBeEmpty();
            restored.Complete(game, At(2)).IsSuccess.ShouldBeTrue();
            repository.Update(restored);
            var tickets = scope.ServiceProvider.GetRequiredService<IMatchmakingTicketsRepository>();
            foreach (var participant in restored.Participants)
            {
                var ticket = (await tickets.GetByIdAsync(participant.TicketId, cancellationToken))!;
                ticket.MarkAssigned(restored.Id).IsSuccess.ShouldBeTrue();
                tickets.Update(ticket);
            }
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var read = readScope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>();
        var row = await read.Set<MatchAssignmentReadDbModel>()
            .Include(value => value.Participants)
            .SingleAsync(value => value.Id == assignment.Id.Value, cancellationToken);
        row.GameId.ShouldBe(game.Value);
        row.Status.ShouldBe(MatchAssignmentStatus.Ready.Name);
        row.Participants.Count.ShouldBe(2);
        var ticketRows = await read.Set<MatchmakingTicketReadDbModel>()
            .Where(value => value.MatchAssignmentId == assignment.Id.Value)
            .ToListAsync(cancellationToken);
        ticketRows.Count.ShouldBe(2);
        ticketRows.ShouldAllBe(value => value.Status == MatchmakingTicketStatus.Assigned.Name);
        foreach (var participant in row.Participants)
        {
            var ticket = ticketRows.Single(value => value.Id == participant.TicketId);
            participant.PlayerId.ShouldBe(ticket.PlayerId);
            participant.BuildSnapshotId.ShouldBe(ticket.BuildSnapshotId);
            participant.Rating.ShouldBe(ticket.Rating);
        }
    }

    [Fact(DisplayName = "Repository should reject stale failure when game creation completed concurrently")]
    public async Task Update_Should_RejectStaleFailure_When_GameCreationCompletedConcurrently()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Domain.MatchAssignments.MatchAssignment assignment;
        await using (var scope = fixture.CreateScope())
        {
            assignment = await SaveAssignmentAsync(scope.ServiceProvider, cancellationToken);
        }

        await using var firstScope = fixture.CreateScope();
        await using var secondScope = fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
        var first = (await firstRepository.GetByIdAsync(assignment.Id, cancellationToken))!;
        var second = (await secondRepository.GetByIdAsync(assignment.Id, cancellationToken))!;
        var game = GameReference.Create(Guid.CreateVersion7()).Value;
        first.Complete(game, At(2)).IsSuccess.ShouldBeTrue();
        firstRepository.Update(first);
        await UnitOfWork(firstScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        second.Fail(MatchmakingReason.Create("Late failure").Value).IsSuccess.ShouldBeTrue();
        secondRepository.Update(second);

        var exception = await Should.ThrowAsync<ConcurrencyException>(() =>
            UnitOfWork(secondScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var retryScope = fixture.CreateScope();
        var retryRepository = retryScope.ServiceProvider.GetRequiredService<IMatchAssignmentsRepository>();
        var retried = (await retryRepository.GetByIdAsync(assignment.Id, cancellationToken))!;
        retried.Fail(MatchmakingReason.Create("Late failure").Value).IsFailure.ShouldBeTrue();
        retried.Complete(game, At(100)).IsSuccess.ShouldBeTrue();
        retried.Status.ShouldBe(MatchAssignmentStatus.Ready);
        retried.GameId.ShouldBe(game);
    }
}
