using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchmakingTickets.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Read.DbModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchmakingTickets.Read.DbModels;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .Matches.Write;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchesRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Repository should preserve pair and game when reloaded between transitions")]
    public async Task GetByIdAsync_Should_PreservePairAndGame_When_ReloadedBetweenTransitions()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Domain.Matches.Match match;
        await using (var scope = fixture.CreateScope())
        {
            match = await SaveMatchAsync(scope.ServiceProvider, cancellationToken);
        }

        var game = GameId.Create(Guid.CreateVersion7()).Value;
        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchesRepository>();
            var restored = (await repository.GetByIdAsync(match.Id, cancellationToken))!;
            restored.Status.ShouldBe(MatchStatus.CreatingGame);
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
        var row = await read.Set<MatchReadDbModel>()
            .Include(value => value.Participants)
            .SingleAsync(value => value.Id == match.Id.Value, cancellationToken);
        row.GameId.ShouldBe(game.Value);
        row.Status.ShouldBe(MatchStatus.Ready.Name);
        row.Participants.Count.ShouldBe(2);
        var ticketRows = await read.Set<MatchmakingTicketReadDbModel>()
            .Where(value => value.MatchId == match.Id.Value)
            .ToListAsync(cancellationToken);
        ticketRows.Count.ShouldBe(2);
        ticketRows.ShouldAllBe(value => value.Status == MatchmakingTicketStatus.Assigned.Name);
        foreach (var participant in row.Participants)
        {
            var ticket = ticketRows.Single(value => value.Id == participant.TicketId);
            participant.PlayerId.ShouldBe(ticket.PlayerId);
            participant.BuildId.ShouldBe(ticket.BuildId);
            participant.Rating.ShouldBe(ticket.Rating);
        }
    }

    [Fact(DisplayName = "Repository should reject stale failure when game creation completed concurrently")]
    public async Task Update_Should_RejectStaleFailure_When_GameCreationCompletedConcurrently()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Domain.Matches.Match match;
        await using (var scope = fixture.CreateScope())
        {
            match = await SaveMatchAsync(scope.ServiceProvider, cancellationToken);
        }

        await using var firstScope = fixture.CreateScope();
        await using var secondScope = fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IMatchesRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IMatchesRepository>();
        var first = (await firstRepository.GetByIdAsync(match.Id, cancellationToken))!;
        var second = (await secondRepository.GetByIdAsync(match.Id, cancellationToken))!;
        var game = GameId.Create(Guid.CreateVersion7()).Value;
        first.Complete(game, At(2)).IsSuccess.ShouldBeTrue();
        firstRepository.Update(first);
        await UnitOfWork(firstScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        second.Fail().IsSuccess.ShouldBeTrue();
        secondRepository.Update(second);

        var exception = await Should.ThrowAsync<ConcurrencyException>(() =>
            UnitOfWork(secondScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var retryScope = fixture.CreateScope();
        var retryRepository = retryScope.ServiceProvider.GetRequiredService<IMatchesRepository>();
        var retried = (await retryRepository.GetByIdAsync(match.Id, cancellationToken))!;
        retried.Fail().IsFailure.ShouldBeTrue();
        retried.Complete(game, At(100)).IsSuccess.ShouldBeTrue();
        retried.Status.ShouldBe(MatchStatus.Ready);
        retried.GameId.ShouldBe(game);
    }
}
