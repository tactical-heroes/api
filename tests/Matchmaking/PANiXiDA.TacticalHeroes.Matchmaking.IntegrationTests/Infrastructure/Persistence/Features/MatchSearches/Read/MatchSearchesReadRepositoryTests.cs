using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchSearches.Read;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchSearchesReadRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Repository should read entire queue when more than one hundred players are waiting")]
    public async Task GetSearchingAsync_Should_ReadEntireQueue_When_MoreThanOneHundredPlayersAreWaiting()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var searches = Enumerable.Range(0, 151).Select(index => CreateSearch(queuedAt: index)).ToArray();
        await using (var writeScope = fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            foreach (var search in searches)
            {
                repository.Add(search);
            }

            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(ct);
        }

        await using var scope = fixture.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMatchSearchesReadRepository>()
            .GetSearchingAsync(At(151).Value, ct);

        result.Count.ShouldBe(151);
        result.First().Id.ShouldBe(searches[0].Id.Value);
        result.Last().Id.ShouldBe(searches[^1].Id.Value);
        result.First().PlayerId.ShouldBe(searches[0].PlayerId.Value);
        result.First().Rating.ShouldBe(searches[0].Rating.Value);
        result.First().Mode.ShouldBe(MatchmakingMode.RankedDuel.Name);
        result.First().QueuedAt.ShouldBe(searches[0].QueuedAt.Value);
        result.First().ExpiresAt.ShouldBe(searches[0].ExpiresAt.Value);
    }

    [Fact(DisplayName = "Repository should exclude inactive searches when queue is read")]
    public async Task GetSearchingAsync_Should_ExcludeInactiveSearches_When_QueueIsRead()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var active = CreateSearch(queuedAt: 10);
        var cancelled = CreateSearch(queuedAt: 10);
        cancelled.Cancel();
        var matched = CreateSearch(queuedAt: 10);
        matched.Matched(At(11));
        var deleted = CreateSearch(queuedAt: 10);
        await using (var writeScope = fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            repository.Add(active);
            repository.Add(cancelled);
            repository.Add(matched);
            repository.Add(deleted);
            repository.Add(CreateSearch());
            repository.Add(CreateSearch(queuedAt: 301));
            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(ct);
            repository.Delete(deleted);
            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(ct);
        }

        await using var scope = fixture.CreateScope();
        var result = await scope.ServiceProvider.GetRequiredService<IMatchSearchesReadRepository>()
            .GetSearchingAsync(At(300).Value, ct);

        result.ShouldHaveSingleItem().Id.ShouldBe(active.Id.Value);
    }

    [Fact(DisplayName = "Repository should find stored aggregate when persisted")]
    public async Task ExistsByIdAsync_Should_FindStoredAggregate_When_Persisted()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        Guid storedId;
        await using (var writeScope = fixture.CreateScope())
        {
            var search = CreateSearch();
            writeScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(search);
            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(cancellationToken);
            storedId = search.Id.Value;
        }

        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesReadRepository>();

        (await repository.ExistsByIdAsync(storedId, cancellationToken)).ShouldBeTrue();
        (await repository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken)).ShouldBeFalse();
        (await repository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "Repository should return false when storage is empty")]
    public async Task AnyAsync_Should_ReturnFalse_When_StorageIsEmpty()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesReadRepository>();

        (await repository.AnyAsync(cancellationToken)).ShouldBeFalse();
    }
}
