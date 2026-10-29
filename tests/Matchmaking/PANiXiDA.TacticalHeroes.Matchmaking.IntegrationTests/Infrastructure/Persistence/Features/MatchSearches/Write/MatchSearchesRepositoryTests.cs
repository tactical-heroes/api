using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;
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
            restored.GameId.ShouldBeNull();
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
        row.GameId.ShouldBeNull();
        row.Mode.ShouldBe(mode);
    }

    [Theory(DisplayName = "Repository should reject second active search when player queues across modes")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task Add_Should_RejectSecondActiveSearch_When_PlayerQueuesAcrossModes(
        bool firstMatched,
        bool secondMatched)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = CreateSearch();
        if (firstMatched)
        {
            first.Matched(At(1)).IsSuccess.ShouldBeTrue();
        }

        await using (var scope = fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(first);
            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var duplicateScope = fixture.CreateScope();
        var second = CreateSearch(first.PlayerId.Value, MatchmakingMode.CasualDuel);
        if (secondMatched)
        {
            second.Matched(At(1)).IsSuccess.ShouldBeTrue();
        }

        duplicateScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .Add(second);
        var exception = await Should.ThrowAsync<Exception>(() =>
            UnitOfWork(duplicateScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.GetBaseException().ShouldBeOfType<PostgresException>().SqlState
            .ShouldBe(PostgresErrorCodes.UniqueViolation);
    }

    [Theory(DisplayName = "Repository should allow new search when previous search became terminal")]
    [InlineData("Cancelled")]
    [InlineData("Expired")]
    [InlineData("Closed")]
    public async Task Add_Should_AllowNewSearch_When_PreviousSearchBecameTerminal(string status)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = CreateSearch();
        await using (var initialScope = fixture.CreateScope())
        {
            var initialRepository = initialScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            initialRepository.Add(first);
            await UnitOfWork(initialScope.ServiceProvider).SaveChangesAsync(cancellationToken);

            if (status == "Closed")
            {
                first.Matched(At(1)).IsSuccess.ShouldBeTrue();
                initialRepository.Update(first);
                await UnitOfWork(initialScope.ServiceProvider).SaveChangesAsync(cancellationToken);
                first.Close().IsSuccess.ShouldBeTrue();
            }
            else if (status == "Expired")
            {
                first.Expire(At(300)).IsSuccess.ShouldBeTrue();
            }
            else
            {
                first.Cancel().IsSuccess.ShouldBeTrue();
            }

            initialRepository.Update(first);
            await UnitOfWork(initialScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var restored = (await repository.GetByIdAsync(first.Id, cancellationToken))!;
        restored.Status.Name.ShouldBe(status);
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

    [Theory(DisplayName = "Repository should preserve shared game when assigned searches are reloaded")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GetByIdAsync_Should_PreserveSharedGame_When_AssignedSearchesAreReloaded(bool closed)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(cancellationToken);
        var first = CreateSearch();
        var second = CreateSearch();
        var gameId = GameId.New();
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
            foreach (var id in new[] { first.Id, second.Id })
            {
                var restored = (await repository.GetByIdAsync(id, cancellationToken))!;
                restored.Matched(At(1)).IsSuccess.ShouldBeTrue();
                restored.AssignGame(gameId).IsSuccess.ShouldBeTrue();
                if (closed)
                {
                    restored.Close().IsSuccess.ShouldBeTrue();
                }

                repository.Update(restored);
            }

            await UnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var readScope = fixture.CreateScope();
        var readRepository = readScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
        var firstRestored = (await readRepository.GetByIdAsync(first.Id, cancellationToken))!;
        var secondRestored = (await readRepository.GetByIdAsync(second.Id, cancellationToken))!;
        firstRestored.GameId.ShouldBe(gameId);
        secondRestored.GameId.ShouldBe(gameId);
        var expectedStatus = closed ? MatchSearchStatus.Closed : MatchSearchStatus.Matched;
        firstRestored.Status.ShouldBe(expectedStatus);
        secondRestored.Status.ShouldBe(expectedStatus);
        var readContext = readScope.ServiceProvider.GetRequiredService<MatchmakingReadDbContext>();
        var rows = await readContext.Set<MatchSearchReadDbModel>()
            .Where(row => row.GameId == gameId.Value)
            .ToListAsync(cancellationToken);
        rows.Count.ShouldBe(2);
        rows.ShouldAllBe(row => row.Status == expectedStatus.Name);
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

    [Theory(DisplayName = "Repository should return occupied search when player has stored history")]
    [InlineData("Searching", true)]
    [InlineData("Matched", true)]
    [InlineData("Cancelled", false)]
    [InlineData("Expired", false)]
    [InlineData("Closed", false)]
    public async Task GetActiveByPlayerIdAsync_Should_ReturnOccupiedSearch_When_PlayerHasStoredHistory(
        string status, bool expected)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var search = CreateSearch();
        if (status is "Matched" or "Closed")
        {
            search.Matched(At(1)).IsSuccess.ShouldBeTrue();
            if (status == "Closed")
            {
                search.Close().IsSuccess.ShouldBeTrue();
            }
        }
        else if (status == "Cancelled")
        {
            search.Cancel().IsSuccess.ShouldBeTrue();
        }
        else if (status == "Expired")
        {
            search.Expire(At(300)).IsSuccess.ShouldBeTrue();
        }

        await using (var writeScope = fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            repository.Add(search);
            repository.Add(CreateSearch());
            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(ct);
        }

        await using var scope = fixture.CreateScope();
        var restored = await scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .GetActiveByPlayerIdAsync(search.PlayerId, ct);

        (restored is not null).ShouldBe(expected);
        if (expected)
        {
            restored!.Id.ShouldBe(search.Id);
            restored.Status.Name.ShouldBe(status);
        }
    }

    [Fact(DisplayName = "Repository should return null when player has no search")]
    public async Task GetActiveByPlayerIdAsync_Should_ReturnNull_When_PlayerHasNoSearch()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();

        var search = await repository.GetActiveByPlayerIdAsync(PlayerId.New(), ct);

        search.ShouldBeNull();
    }

    [Fact(DisplayName = "Repository should return null when active search is deleted")]
    public async Task GetActiveByPlayerIdAsync_Should_ReturnNull_When_ActiveSearchIsDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var search = CreateSearch();
        await using (var writeScope = fixture.CreateScope())
        {
            writeScope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>().Add(search);
            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(ct);
            writeScope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>().Remove(search);
            await UnitOfWork(writeScope.ServiceProvider).SaveChangesAsync(ct);
        }

        await using var scope = fixture.CreateScope();
        var restored = await scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>()
            .GetActiveByPlayerIdAsync(search.PlayerId, ct);

        restored.ShouldBeNull();
        var context = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
        var exists = await context.Set<Domain.MatchSearches.MatchSearch>()
            .IgnoreQueryFilters()
            .AnyAsync(row => row.Id == search.Id, ct);
        exists.ShouldBeTrue();
    }
}
