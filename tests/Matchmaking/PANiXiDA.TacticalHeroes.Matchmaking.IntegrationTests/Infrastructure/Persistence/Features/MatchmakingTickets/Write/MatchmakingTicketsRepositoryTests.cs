using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read.DbModels;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchmakingTickets.Write;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchmakingTicketsRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Repository should preserve selection when reloaded")]
    public async Task GetByIdAsync_Should_PreserveSelection_When_Reloaded()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var ticket = Ticket();
        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMatchmakingTicketsRepository>().Add(ticket);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchmakingTicketsRepository>();
            var restored = (await repository.GetByIdAsync(ticket.Id, cancellationToken))!;
            restored.PlayerId.ShouldBe(ticket.PlayerId);
            restored.BuildSnapshotId.ShouldBe(ticket.BuildSnapshotId);
            restored.Rating.ShouldBe(ticket.Rating);
            restored.Mode.ShouldBe(ticket.Mode);
            restored.ExpiresAt.ShouldBe(ticket.ExpiresAt);
            restored.Cancel().IsSuccess.ShouldBeTrue();
            repository.Update(restored);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var read = readScope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>();
        var row = await read.Set<MatchmakingTicketReadDbModel>()
            .SingleAsync(value => value.Id == ticket.Id.Value, cancellationToken);
        row.Status.ShouldBe(MatchmakingTicketStatus.Cancelled.Name);
        row.PlayerId.ShouldBe(ticket.PlayerId.Value);
        row.BuildSnapshotId.ShouldBe(ticket.BuildSnapshotId.Value);
    }

    [Fact(DisplayName = "Repository should reject second active ticket when player queues across modes")]
    public async Task Add_Should_RejectSecondActiveTicket_When_PlayerQueuesAcrossModes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = Ticket();
        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMatchmakingTicketsRepository>().Add(first);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var duplicateScope = fixture.CreateScope();
        duplicateScope.ServiceProvider.GetRequiredService<IMatchmakingTicketsRepository>()
            .Add(Ticket(first.PlayerId.Value, "other"));
        var exception = await Should.ThrowAsync<Exception>(() =>
            UnitOfWork(duplicateScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.GetBaseException().ShouldBeOfType<PostgresException>().SqlState
            .ShouldBe(PostgresErrorCodes.UniqueViolation);
    }

    [Fact(DisplayName = "Repository should allow new search when previous ticket was cancelled")]
    public async Task Add_Should_AllowNewSearch_When_PreviousTicketWasCancelled()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = Ticket();
        first.Cancel().IsSuccess.ShouldBeTrue();
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchmakingTicketsRepository>();
        repository.Add(first);
        repository.Add(Ticket(first.PlayerId.Value));

        await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);

        var context = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
        (await context.Set<Domain.MatchmakingTickets.MatchmakingTicket>().CountAsync(cancellationToken)).ShouldBe(2);
    }
}
