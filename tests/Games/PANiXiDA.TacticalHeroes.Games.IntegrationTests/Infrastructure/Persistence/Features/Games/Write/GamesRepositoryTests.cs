using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Games.Domain.Battles;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Enumerations;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.ValueObjects;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Features.Games.Read.DbModels;
using PANiXiDA.TacticalHeroes.Games.IntegrationTests.Games;

namespace PANiXiDA.TacticalHeroes.Games.IntegrationTests.Infrastructure.Persistence.Features.Games.Write;

public sealed class GamesRepositoryTests(IntegrationTestFixture fixture) : IntegrationTestBase(fixture)
{
    [Theory(DisplayName = "Game should restore its participants when mode is supported")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public async Task GetByIdAsync_Should_RestoreParticipants_When_ModeIsSupported(string mode)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var game = GameTestData.Create(GameMode.Create(mode).Value);
        await AddGamesAsync([game], cancellationToken);

        await using var scope = Fixture.CreateScope();
        var restored = await scope.ServiceProvider.GetRequiredService<IGamesRepository>()
            .GetByIdAsync(game.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Mode.ShouldBe(game.Mode);
        restored.Status.ShouldBe(GameStatus.WaitingForPlayers);
        restored.Result.ShouldBeNull();
        restored.BattleId.ShouldBeNull();
        restored.Participants.Count.ShouldBe(2);
        foreach (var expected in game.Participants)
        {
            var actual = restored.Participants.Single(participant => participant.Id == expected.Id);
            actual.GameId.ShouldBe(game.Id);
            actual.PlayerId.ShouldBe(expected.PlayerId);
            actual.BuildId.ShouldBe(expected.BuildId);
            actual.Rating.ShouldBe(expected.Rating);
        }
    }

    [Theory(DisplayName = "Game should restore lifecycle and result when state is persisted")]
    [InlineData("InProgress")]
    [InlineData("Victory")]
    [InlineData("Draw")]
    [InlineData("Cancelled")]
    public async Task Update_Should_RestoreLifecycleAndResult_When_StateIsPersisted(string state)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var game = GameTestData.Create();
        var battleId = BattleId.New();
        await AddGamesAsync([game], cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IGamesRepository>();
            var loaded = (await repository.GetByIdAsync(game.Id, cancellationToken))!;
            if (state == "Cancelled")
            {
                loaded.Cancel(GameTestData.At(20)).IsSuccess.ShouldBeTrue();
            }
            else
            {
                loaded.Start(battleId, GameTestData.At(10)).IsSuccess.ShouldBeTrue();
                if (state is "Victory" or "Draw")
                {
                    var result = state == "Draw"
                        ? GameResult.Draw()
                        : GameResult.Victory(loaded.Participants.First().PlayerId);

                    loaded.Finish(battleId, result, GameTestData.At(20)).IsSuccess.ShouldBeTrue();
                }
            }

            repository.Update(loaded);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IGamesRepository>()
            .GetByIdAsync(game.Id, cancellationToken);
        var readContext = verificationScope.ServiceProvider.GetRequiredService<GamesReadDbContext>();
        var readModel = await readContext.Set<GameReadDbModel>()
            .Include(item => item.Participants)
            .SingleAsync(item => item.Id == game.Id.Value, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Participants.Count.ShouldBe(2);
        readModel.Participants.Count.ShouldBe(2);
        readModel.Status.ShouldBe(restored.Status.Name);
        readModel.StartedAt.ShouldBe(restored.StartedAt?.Value);
        readModel.FinishedAt.ShouldBe(restored.FinishedAt?.Value);
        readModel.BattleId.ShouldBe(restored.BattleId?.Value);
        readModel.ResultIsDraw.ShouldBe(restored.Result?.IsDraw);
        readModel.ResultWinnerPlayerId.ShouldBe(restored.Result?.WinnerPlayerId?.Value);
        if (state is "Victory" or "Draw")
        {
            restored.Status.ShouldBe(GameStatus.Finished);
            restored.FinishedAt.ShouldBe(GameTestData.At(20));
            restored.Result.ShouldNotBeNull();
            restored.Result.IsDraw.ShouldBe(state == "Draw");
            if (state == "Victory")
            {
                restored.Participants.Any(participant => participant.PlayerId == restored.Result.WinnerPlayerId)
                    .ShouldBeTrue();
            }

            restored.Finish(battleId, restored.Result, GameTestData.At(30)).IsSuccess.ShouldBeTrue();
            restored.FinishedAt.ShouldBe(GameTestData.At(20));
            restored.GetDomainEvents().ShouldBeEmpty();
        }
        else
        {
            restored.Result.ShouldBeNull();
            restored.Status.ShouldBe(state == "Cancelled" ? GameStatus.Cancelled : GameStatus.InProgress);
        }
    }

    [Fact(DisplayName = "Game persistence should reject a second game when battle is already assigned")]
    public async Task Update_Should_RejectSecondGame_When_BattleIsAlreadyAssigned()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var first = GameTestData.Create();
        var second = GameTestData.Create();
        var battleId = BattleId.New();
        first.Start(battleId, GameTestData.At(10));
        await AddGamesAsync([first, second], cancellationToken);
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IGamesRepository>();
        var loaded = (await repository.GetByIdAsync(second.Id, cancellationToken))!;
        loaded.Start(battleId, GameTestData.At(10));
        repository.Update(loaded);
        var context = scope.ServiceProvider.GetRequiredService<GamesWriteDbContext>();

        var exception = await Should.ThrowAsync<DbUpdateException>(
            () => context.SaveChangesAsync(cancellationToken));

        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
        postgresException.ConstraintName.ShouldBe("ix_games_battle_id");
    }

    [Fact(DisplayName = "Game persistence should reject stale cancellation when start was saved first")]
    public async Task Update_Should_RejectStaleCancellation_When_StartWasSavedFirst()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var game = GameTestData.Create();
        var battleId = BattleId.New();
        await AddGamesAsync([game], cancellationToken);
        await using var firstScope = Fixture.CreateScope();
        await using var secondScope = Fixture.CreateScope();
        var firstRepository = firstScope.ServiceProvider.GetRequiredService<IGamesRepository>();
        var secondRepository = secondScope.ServiceProvider.GetRequiredService<IGamesRepository>();
        var first = (await firstRepository.GetByIdAsync(game.Id, cancellationToken))!;
        var second = (await secondRepository.GetByIdAsync(game.Id, cancellationToken))!;
        first.Start(battleId, GameTestData.At(10));
        firstRepository.Update(first);
        await GetUnitOfWork(firstScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        second.Cancel(GameTestData.At(20));
        secondRepository.Update(second);

        var exception = await Should.ThrowAsync<ConcurrencyException>(
            () => GetUnitOfWork(secondScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IGamesRepository>()
            .GetByIdAsync(game.Id, cancellationToken);
        restored.ShouldNotBeNull();
        restored.Status.ShouldBe(GameStatus.InProgress);
        restored.BattleId.ShouldBe(battleId);
        restored.FinishedAt.ShouldBeNull();
    }

    [Fact(DisplayName = "Game should remain waiting when the start transaction is rolled back")]
    public async Task Update_Should_RestoreWaitingState_When_TheStartTransactionIsRolledBack()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var game = GameTestData.Create();
        await AddGamesAsync([game], cancellationToken);
        await using (var scope = Fixture.CreateScope())
        {
            var unitOfWork = GetUnitOfWork(scope.ServiceProvider);
            await unitOfWork.BeginTransactionAsync(cancellationToken);
            var repository = scope.ServiceProvider.GetRequiredService<IGamesRepository>();
            var loaded = (await repository.GetByIdAsync(game.Id, cancellationToken))!;
            loaded.Start(BattleId.New(), GameTestData.At(10));
            repository.Update(loaded);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await unitOfWork.RollbackTransactionAsync(cancellationToken);
            await unitOfWork.DisposeTransactionAsync();
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IGamesRepository>()
            .GetByIdAsync(game.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Status.ShouldBe(GameStatus.WaitingForPlayers);
        restored.BattleId.ShouldBeNull();
        restored.StartedAt.ShouldBeNull();
        restored.Participants.Count.ShouldBe(2);
    }

    private static IUnitOfWork GetUnitOfWork(IServiceProvider services)
    {
        return services.GetRequiredKeyedService<IUnitOfWork>(typeof(GamesWriteDbContext));
    }

    private async Task AddGamesAsync(Game[] games, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IGamesRepository>();
        foreach (var game in games)
        {
            repository.Add(game);
        }

        await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
    }
}
