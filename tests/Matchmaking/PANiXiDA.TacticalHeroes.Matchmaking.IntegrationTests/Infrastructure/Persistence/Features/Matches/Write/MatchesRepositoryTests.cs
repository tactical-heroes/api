using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Matches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.Matches.Read.DbModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

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
            var searches = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            foreach (var participant in restored.Participants)
            {
                var search = (await searches.GetByIdAsync(participant.MatchSearchId, cancellationToken))!;
                search.MarkAssigned(restored.Id).IsSuccess.ShouldBeTrue();
                searches.Update(search);
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
        var searchRows = await read.Set<MatchSearchReadDbModel>()
            .Where(value => value.MatchId == match.Id.Value)
            .ToListAsync(cancellationToken);
        searchRows.Count.ShouldBe(2);
        searchRows.ShouldAllBe(value => value.Status == MatchSearchStatus.Assigned.Name);
        foreach (var participant in row.Participants)
        {
            var search = searchRows.Single(value => value.Id == participant.MatchSearchId);
            participant.PlayerId.ShouldBe(search.PlayerId);
            participant.BuildId.ShouldBe(search.BuildId);
            participant.Rating.ShouldBe(search.Rating);
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
