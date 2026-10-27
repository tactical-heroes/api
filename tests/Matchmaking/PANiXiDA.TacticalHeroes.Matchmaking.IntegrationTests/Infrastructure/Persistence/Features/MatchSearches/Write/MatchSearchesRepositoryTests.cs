using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.Core.Application.Persistence.Errors;

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
    [Theory(DisplayName = "Repository should preserve selection when reloaded")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public async Task GetByIdAsync_Should_PreserveSelection_When_Reloaded(string mode)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var search = CreateSearch(mode: MatchmakingMode.Create(mode).Value);
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
            restored.ExpiresAt.Value.ShouldBe(restored.QueuedAt.Value.AddMinutes(5));
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
        row.Mode.ShouldBe(mode);
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
            .Add(CreateSearch(first.PlayerId.Value, MatchmakingMode.CasualDuel));
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

    [Fact(DisplayName = "Repository should preserve both matched searches when saved together")]
    public async Task GetByIdAsync_Should_PreserveBothMatchedSearches_When_SavedTogether()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = CreateSearch();
        var second = CreateSearch();
        first.MatchWith(second, At(1)).IsSuccess.ShouldBeTrue();
        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            repository.Add(first);
            repository.Add(second);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var read = readScope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>();
        var rows = await read.Set<MatchSearchReadDbModel>().ToListAsync(cancellationToken);

        rows.Count.ShouldBe(2);
        rows.ShouldAllBe(row => row.Status == MatchSearchStatus.Matched.Name);
        rows.Single(row => row.Id == first.Id.Value).BuildId.ShouldBe(first.BuildId.Value);
        rows.Single(row => row.Id == second.Id.Value).BuildId.ShouldBe(second.BuildId.Value);
    }

    [Fact(DisplayName = "Repository should roll back the losing pair when matchers compete for one search")]
    public async Task Update_Should_RollBackTheLosingPair_When_MatchersCompeteForOneSearch()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var shared = CreateSearch();
        var firstPeer = CreateSearch();
        var secondPeer = CreateSearch();
        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            repository.Add(shared);
            repository.Add(firstPeer);
            repository.Add(secondPeer);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }
        await using var firstScope = fixture.CreateScope();
        await using var secondScope = fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var firstShared = (await firstRepository.GetByIdAsync(shared.Id, cancellationToken))!;
        var secondShared = (await secondRepository.GetByIdAsync(shared.Id, cancellationToken))!;
        var first = (await firstRepository.GetByIdAsync(firstPeer.Id, cancellationToken))!;
        var second = (await secondRepository.GetByIdAsync(secondPeer.Id, cancellationToken))!;
        firstShared.MatchWith(first, At(1)).IsSuccess.ShouldBeTrue();
        secondShared.MatchWith(second, At(1)).IsSuccess.ShouldBeTrue();
        firstRepository.Update(firstShared);
        firstRepository.Update(first);
        secondRepository.Update(secondShared);
        secondRepository.Update(second);
        await UnitOfWork(firstScope.ServiceProvider).SaveChangesAsync(cancellationToken);

        var exception = await Should.ThrowAsync<ConcurrencyException>(() =>
            UnitOfWork(secondScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var readScope = fixture.CreateScope();
        var repositoryAfterRace = readScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        (await repositoryAfterRace.GetByIdAsync(shared.Id, cancellationToken))!.Status
            .ShouldBe(MatchSearchStatus.Matched);
        (await repositoryAfterRace.GetByIdAsync(firstPeer.Id, cancellationToken))!.Status
            .ShouldBe(MatchSearchStatus.Matched);
        (await repositoryAfterRace.GetByIdAsync(secondPeer.Id, cancellationToken))!.Status
            .ShouldBe(MatchSearchStatus.Searching);
    }

    [Fact(DisplayName = "Repository should restore both searching states when transaction is rolled back")]
    public async Task Update_Should_RestoreBothSearchingStates_When_TransactionIsRolledBack()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = CreateSearch();
        var second = CreateSearch();
        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            repository.Add(first);
            repository.Add(second);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using (var scope = fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            var restoredFirst = (await repository.GetByIdAsync(first.Id, cancellationToken))!;
            var restoredSecond = (await repository.GetByIdAsync(second.Id, cancellationToken))!;
            var unitOfWork = UnitOfWork(scope.ServiceProvider);
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                restoredFirst.MatchWith(restoredSecond, At(1)).IsSuccess.ShouldBeTrue();
                repository.Update(restoredFirst);
                repository.Update(restoredSecond);
                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                await unitOfWork.DisposeTransactionAsync();
            }
        }

        await using var readScope = fixture.CreateScope();
        var rows = await readScope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>()
            .Set<MatchSearchReadDbModel>()
            .ToListAsync(cancellationToken);
        rows.Count.ShouldBe(2);
        rows.ShouldAllBe(row => row.Status == MatchSearchStatus.Searching.Name);
    }
}
