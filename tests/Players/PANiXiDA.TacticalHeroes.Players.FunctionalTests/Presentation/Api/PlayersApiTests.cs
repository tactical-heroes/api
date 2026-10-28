using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.Players.Contracts.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.FunctionalTests.Presentation.Api;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class PlayersApiTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Module API should return persisted profile when user has a player")]
    public async Task GetByUserIdAsync_Should_ReturnPersistedProfile_When_UserHasAPlayer()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = Player.Create(UserId.New());
        player.ChangeRating(Rating.Create(1742));
        await using (var writeScope = fixture.Services.CreateAsyncScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            repository.Add(Player.Create(UserId.New()));
            repository.Add(player);
            await writeScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
                .SaveChangesAsync(ct);
        }

        await using var scope = fixture.Services.CreateAsyncScope();
        var profile = (await scope.ServiceProvider.GetRequiredService<IPlayersApi>()
            .GetByUserIdAsync(player.UserId.Value, ct))!;

        profile.Id.ShouldBe(player.Id.Value);
        profile.Rating.ShouldBe(1742);
        profile.Id.ShouldNotBe(player.UserId.Value);
    }

    [Fact(DisplayName = "Module API should return null when player is missing")]
    public async Task GetByUserIdAsync_Should_ReturnNull_When_PlayerIsMissing()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        await using var scope = fixture.Services.CreateAsyncScope();

        var profile = await scope.ServiceProvider.GetRequiredService<IPlayersApi>()
            .GetByUserIdAsync(Guid.CreateVersion7(), ct);

        profile.ShouldBeNull();
    }

    [Fact(DisplayName = "Module API should return null when player was deleted")]
    public async Task GetByUserIdAsync_Should_ReturnNull_When_PlayerWasDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var player = Player.Create(UserId.New());
        await using (var writeScope = fixture.Services.CreateAsyncScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            var unitOfWork = writeScope.ServiceProvider
                .GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext));
            repository.Add(player);
            await unitOfWork.SaveChangesAsync(ct);
            repository.Delete(player);
            await unitOfWork.SaveChangesAsync(ct);
        }

        await using var scope = fixture.Services.CreateAsyncScope();
        var profile = await scope.ServiceProvider.GetRequiredService<IPlayersApi>()
            .GetByUserIdAsync(player.UserId.Value, ct);

        profile.ShouldBeNull();
    }
}
