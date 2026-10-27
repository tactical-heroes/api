using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchSearches.Read;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchSearchesReadRepositoryTests(IntegrationTestFixture fixture)
{
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
