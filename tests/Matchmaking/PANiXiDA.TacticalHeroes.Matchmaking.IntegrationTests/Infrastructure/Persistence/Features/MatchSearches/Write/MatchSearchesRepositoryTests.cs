using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Features.MatchSearches.Read.DbModels;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Persistence.Features
    .MatchSearches.Write;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchSearchesRepositoryTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "Repository should preserve selection when reloaded")]
    public async Task GetByIdAsync_Should_PreserveSelection_When_Reloaded()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var search = CreateSearch();
        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(search);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            var restored = (await repository.GetByIdAsync(search.Id, cancellationToken))!;
            restored.PlayerId.ShouldBe(search.PlayerId);
            restored.BuildId.ShouldBe(search.BuildId);
            restored.Rating.ShouldBe(search.Rating);
            restored.Mode.ShouldBe(search.Mode);
            restored.ExpiresAt.ShouldBe(search.ExpiresAt);
            restored.Cancel().IsSuccess.ShouldBeTrue();
            repository.Update(restored);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var read = readScope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>();
        var row = await read.Set<MatchSearchReadDbModel>()
            .SingleAsync(value => value.Id == search.Id.Value, cancellationToken);
        row.Status.ShouldBe(MatchSearchStatus.Cancelled.Name);
        row.PlayerId.ShouldBe(search.PlayerId.Value);
        row.BuildId.ShouldBe(search.BuildId.Value);
    }

    [Fact(DisplayName = "Repository should reject second active search when player queues across modes")]
    public async Task Add_Should_RejectSecondActiveSearch_When_PlayerQueuesAcrossModes()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = CreateSearch();
        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(first);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var duplicateScope = fixture.CreateScope();
        duplicateScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .Add(CreateSearch(first.PlayerId.Value, "other"));
        var exception = await Should.ThrowAsync<Exception>(() =>
            UnitOfWork(duplicateScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.GetBaseException().ShouldBeOfType<PostgresException>().SqlState
            .ShouldBe(PostgresErrorCodes.UniqueViolation);
    }

    [Fact(DisplayName = "Repository should allow new search when previous search was cancelled")]
    public async Task Add_Should_AllowNewSearch_When_PreviousSearchWasCancelled()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = CreateSearch();
        first.Cancel().IsSuccess.ShouldBeTrue();
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        repository.Add(first);
        repository.Add(CreateSearch(first.PlayerId.Value));

        await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);

        var context = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
        (await context.Set<Domain.MatchSearches.MatchSearch>().CountAsync(cancellationToken)).ShouldBe(2);
    }
}
