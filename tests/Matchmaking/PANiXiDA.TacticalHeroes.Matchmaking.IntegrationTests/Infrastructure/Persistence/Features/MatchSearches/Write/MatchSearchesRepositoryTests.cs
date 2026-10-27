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

    [Fact(DisplayName = "Repository should preserve matched search when reloaded")]
    public async Task GetByIdAsync_Should_PreserveMatchedSearch_When_Reloaded()
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

            restored.Matched(At(1)).IsSuccess.ShouldBeTrue();

            repository.Update(restored);

            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var read = readScope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>();
        var row = await read.Set<MatchSearchReadDbModel>()
            .SingleAsync(value => value.Id == search.Id.Value, cancellationToken);

        row.Status.ShouldBe(MatchSearchStatus.Matched.Name);
        row.PlayerId.ShouldBe(search.PlayerId.Value);
        row.BuildId.ShouldBe(search.BuildId.Value);
    }

    [Fact(DisplayName = "Repository should reject stale cancellation when search was matched concurrently")]
    public async Task Update_Should_RejectStaleCancellation_When_SearchWasMatchedConcurrently()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var search = CreateSearch();
        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(search);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }
        await using var firstScope = fixture.CreateScope();
        await using var secondScope = fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var first = (await firstRepository.GetByIdAsync(search.Id, cancellationToken))!;
        var second = (await secondRepository.GetByIdAsync(search.Id, cancellationToken))!;

        first.Matched(At(1)).IsSuccess.ShouldBeTrue();

        firstRepository.Update(first);

        await UnitOfWork(firstScope.ServiceProvider).SaveChangesAsync(cancellationToken);

        second.Cancel().IsSuccess.ShouldBeTrue();

        secondRepository.Update(second);

        var exception = await Should.ThrowAsync<ConcurrencyException>(() =>
            UnitOfWork(secondScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var readScope = fixture.CreateScope();
        var repositoryAfterRace = readScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var restored = (await repositoryAfterRace.GetByIdAsync(search.Id, cancellationToken))!;
        restored.Status.ShouldBe(MatchSearchStatus.Matched);
    }

    [Fact(DisplayName = "Repository should restore searching state when transaction is rolled back")]
    public async Task Update_Should_RestoreSearchingState_When_TransactionIsRolledBack()
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
            var unitOfWork = UnitOfWork(scope.ServiceProvider);
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                restored.Matched(At(1)).IsSuccess.ShouldBeTrue();

                repository.Update(restored);

                await unitOfWork.SaveChangesAsync(cancellationToken);
            }
            finally
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
                await unitOfWork.DisposeTransactionAsync();
            }
        }

        await using var readScope = fixture.CreateScope();
        var repositoryAfterRollback = readScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var row = (await repositoryAfterRollback.GetByIdAsync(search.Id, cancellationToken))!;
        row.Status.ShouldBe(MatchSearchStatus.Searching);
    }
}
