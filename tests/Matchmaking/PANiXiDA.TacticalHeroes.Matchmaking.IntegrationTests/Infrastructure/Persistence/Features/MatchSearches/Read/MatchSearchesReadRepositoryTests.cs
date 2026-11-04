using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

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

    [Fact(DisplayName = "Repository should order searches when timestamps run backwards")]
    public async Task GetStateByPlayerIdAsync_Should_OrderSearches_When_TimestampsRunBackwards()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var reads = scope.ServiceProvider.GetRequiredService<IMatchSearchesReadRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext));
        var first = CreateSearch();
        repository.Add(first);
        await unitOfWork.SaveChangesAsync(ct);
        var initial = (await reads.GetStateByPlayerIdAsync(first.PlayerId.Value, ct)).ShouldNotBeNull();
        initial.MatchSearchId.ShouldBe(first.Id.Value);
        initial.Status.ShouldBe("Searching");
        first.Cancel().IsSuccess.ShouldBeTrue();
        repository.Update(first);
        await unitOfWork.SaveChangesAsync(ct);
        var cancelled = (await reads.GetStateByPlayerIdAsync(first.PlayerId.Value, ct)).ShouldNotBeNull();
        cancelled.Status.ShouldBe("Cancelled");
        var newer = MatchSearch.Create(
            first.PlayerId,
            first.BuildId,
            first.Rating,
            first.Mode,
            MatchmakingTimestamp.Create(first.QueuedAt.Value.AddHours(-1))).Value;
        repository.Add(newer);
        await unitOfWork.SaveChangesAsync(ct);
        var latest = (await reads.GetStateByPlayerIdAsync(first.PlayerId.Value, ct)).ShouldNotBeNull();
        latest.MatchSearchId.ShouldBe(newer.Id.Value);
        first.Cancel().IsSuccess.ShouldBeTrue();
        repository.Update(first);
        await unitOfWork.SaveChangesAsync(ct);

        (await reads.GetStateByPlayerIdAsync(first.PlayerId.Value, ct)).ShouldBe(latest);
        newer.Cancel().IsSuccess.ShouldBeTrue();
        repository.Update(newer);
        await unitOfWork.SaveChangesAsync(ct);
        var terminal = (await reads.GetStateByPlayerIdAsync(first.PlayerId.Value, ct)).ShouldNotBeNull();
        terminal.MatchSearchId.ShouldBe(newer.Id.Value);
        terminal.Status.ShouldBe("Cancelled");
    }
}
