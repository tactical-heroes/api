using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.ResultPattern;

using PANiXiDA.TacticalHeroes.Players.Contracts.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.FunctionalTests.Presentation.Internal;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class PlayersApiTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Module API should return persisted player when user has a player")]
    public async Task GetByUserIdAsync_Should_ReturnPersistedPlayer_When_UserHasAPlayer()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var createdPlayer = Player.Create(UserId.New());
        createdPlayer.ChangeRating(Rating.Create(1742));
        await using (var writeScope = fixture.Services.CreateAsyncScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            repository.Add(Player.Create(UserId.New()));
            repository.Add(createdPlayer);
            await writeScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
                .SaveChangesAsync(ct);
        }

        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IPlayersApi>()
            .GetByUserIdAsync(createdPlayer.UserId.Value, ct);

        result.IsSuccess.ShouldBeTrue();
        result.Value.Id.ShouldBe(createdPlayer.Id.Value);
        result.Value.Rating.ShouldBe(1742);
        result.Value.Id.ShouldNotBe(createdPlayer.UserId.Value);
    }

    [Fact(DisplayName = "Module API should return not found when player is missing")]
    public async Task GetByUserIdAsync_Should_ReturnNotFound_When_PlayerIsMissing()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        await using var scope = fixture.Services.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<IPlayersApi>()
            .GetByUserIdAsync(Guid.CreateVersion7(), ct);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact(DisplayName = "Module API should return not found when player was deleted")]
    public async Task GetByUserIdAsync_Should_ReturnNotFound_When_PlayerWasDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var createdPlayer = Player.Create(UserId.New());
        await using (var writeScope = fixture.Services.CreateAsyncScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            var unitOfWork = writeScope.ServiceProvider
                .GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext));
            repository.Add(createdPlayer);
            await unitOfWork.SaveChangesAsync(ct);
            repository.Delete(createdPlayer);
            await unitOfWork.SaveChangesAsync(ct);
        }

        await using var scope = fixture.Services.CreateAsyncScope();
        var result = await scope.ServiceProvider.GetRequiredService<IPlayersApi>()
            .GetByUserIdAsync(createdPlayer.UserId.Value, ct);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
    }

    [Fact(DisplayName = "Module API should return validation error when user id is empty")]
    public async Task GetByUserIdAsync_Should_ReturnValidationError_When_UserIdIsEmpty()
    {
        await using var scope = fixture.Services.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<IPlayersApi>()
            .GetByUserIdAsync(Guid.Empty, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Validation);
    }
}
