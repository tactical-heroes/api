using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Specifications;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchSearches.Write;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchSearchExpirationRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Repository should select overdue searching aggregates when expiration is queried")]
    public async Task GetAllAsync_Should_SelectOverdueSearchingAggregates_When_ExpirationIsQueried()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var overdue = CreateSearch(queuedAt: -1);
        var boundary = CreateSearch(mode: MatchmakingMode.CasualDuel);
        var active = CreateSearch(queuedAt: 1);
        var cancelled = CreateSearch();
        cancelled.Cancel().IsSuccess.ShouldBeTrue();
        var matched = CreateSearch();
        matched.Matched(At(1)).IsSuccess.ShouldBeTrue();
        var closed = CreateSearch();
        closed.Matched(At(1)).IsSuccess.ShouldBeTrue();
        closed.Close().IsSuccess.ShouldBeTrue();
        var expired = CreateSearch();
        expired.Expire(At(300)).IsSuccess.ShouldBeTrue();
        var deleted = CreateSearch();
        await using (var writeScope = fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            foreach (var search in new[] { overdue, boundary, active, cancelled, matched, closed, expired, deleted })
            {
                repository.Add(search);
            }

            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(ct);
            repository.Delete(deleted);
            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(ct);
        }

        await using var scope = fixture.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .GetAllAsync(new OverdueMatchSearchSpecification(At(300)), ct);

        result.Select(search => search.Id).ShouldBe([overdue.Id, boundary.Id]);
        var context = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
        var entries = context.ChangeTracker.Entries<Domain.MatchSearches.MatchSearch>().ToArray();
        entries.Length.ShouldBe(2);
        entries.ShouldAllBe(entry => entry.State == EntityState.Unchanged);
    }

    [Theory(DisplayName = "Repository should preserve winning transition when expiration races with matching")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Update_Should_PreserveWinningTransition_When_ExpirationRacesWithMatching(bool matchWins)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var search = CreateSearch();
        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(search);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(ct);
        }

        await using var matchingScope = fixture.CreateScope();
        await using var expirationScope = fixture.CreateScope();
        var matchingRepository = matchingScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var expirationRepository = expirationScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var matching = (await matchingRepository.GetByIdAsync(search.Id, ct))!;
        matching.Matched(At(299)).IsSuccess.ShouldBeTrue();
        matchingRepository.Update(matching);
        var expiring = (await expirationRepository.GetAllAsync(new OverdueMatchSearchSpecification(At(300)), ct))
            .Single();
        expiring.Expire(At(300)).IsSuccess.ShouldBeTrue();
        expirationRepository.Update(expiring);
        var winningServices = matchWins ? matchingScope.ServiceProvider : expirationScope.ServiceProvider;
        var losingServices = matchWins ? expirationScope.ServiceProvider : matchingScope.ServiceProvider;

        await UnitOfWork(winningServices).SaveChangesAsync(ct);
        var exception = await Should.ThrowAsync<ConcurrencyException>(() =>
            UnitOfWork(losingServices).SaveChangesAsync(ct));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var readScope = fixture.CreateScope();
        var stored = (await readScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .GetByIdAsync(search.Id, ct))!;
        stored.Status.ShouldBe(matchWins ? MatchSearchStatus.Matched : MatchSearchStatus.Expired);
    }
}
