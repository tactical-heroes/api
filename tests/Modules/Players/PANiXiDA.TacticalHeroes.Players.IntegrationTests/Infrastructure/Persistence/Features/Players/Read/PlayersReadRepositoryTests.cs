using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Players.IntegrationTests.Infrastructure.Persistence.Features.Players.Read;

public sealed class PlayersReadRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Read repository should report persisted players when players exist")]
    public async Task ExistsByIdAsync_Should_ReportPersistedPlayers_When_PlayersExist()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var player = Player.Create(UserId.New());
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IPlayersRepository>().Add(player);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var repository = verificationScope.ServiceProvider.GetRequiredService<IPlayersReadRepository>();

        (await repository.ExistsByIdAsync(player.Id.Value, cancellationToken)).ShouldBeTrue();
        (await repository.ExistsByIdAsync(Guid.CreateVersion7(), cancellationToken)).ShouldBeFalse();
    }

    [Fact(DisplayName = "Read repository should detect players when the first player is saved")]
    public async Task AnyAsync_Should_DetectPlayers_When_TheFirstPlayerIsSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IPlayersReadRepository>();
            (await repository.AnyAsync(cancellationToken)).ShouldBeFalse();
        }

        var player = Player.Create(UserId.New());
        await using (var scope = Fixture.CreateScope())
        {
            scope.ServiceProvider.GetRequiredService<IPlayersRepository>().Add(player);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
                .SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var populatedRepository = verificationScope.ServiceProvider.GetRequiredService<IPlayersReadRepository>();

        (await populatedRepository.AnyAsync(cancellationToken)).ShouldBeTrue();
    }

    [Fact(DisplayName = "Read repository should return matching player when user has a player")]
    public async Task GetByUserIdAsync_Should_ReturnMatchingPlayer_When_UserHasAPlayer()
    {
        var ct = TestContext.Current.CancellationToken;
        var createdPlayer = Player.Create(UserId.New());
        createdPlayer.ChangeRating(Rating.Create(1742));
        await using (var writeScope = Fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            repository.Add(Player.Create(UserId.New()));
            repository.Add(createdPlayer);
            await writeScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
                .SaveChangesAsync(ct);
        }

        await using var scope = Fixture.CreateScope();
        var player = (await scope.ServiceProvider.GetRequiredService<IPlayersReadRepository>()
            .GetByUserIdAsync(createdPlayer.UserId.Value, ct))!;

        player.Id.ShouldBe(createdPlayer.Id.Value);
        player.Rating.ShouldBe(1742);
    }

    [Fact(DisplayName = "Read repository should return null when user has no player")]
    public async Task GetByUserIdAsync_Should_ReturnNull_When_UserHasNoPlayer()
    {
        await using var scope = Fixture.CreateScope();

        var player = await scope.ServiceProvider.GetRequiredService<IPlayersReadRepository>()
            .GetByUserIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        player.ShouldBeNull();
    }

    [Fact(DisplayName = "Read repository should return null when player was deleted")]
    public async Task GetByUserIdAsync_Should_ReturnNull_When_PlayerWasDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var createdPlayer = Player.Create(UserId.New());
        await using (var writeScope = Fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            var unitOfWork = writeScope.ServiceProvider
                .GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext));
            repository.Add(createdPlayer);
            await unitOfWork.SaveChangesAsync(ct);
            repository.Delete(createdPlayer);
            await unitOfWork.SaveChangesAsync(ct);
        }

        await using var scope = Fixture.CreateScope();
        var player = await scope.ServiceProvider.GetRequiredService<IPlayersReadRepository>()
            .GetByUserIdAsync(createdPlayer.UserId.Value, ct);

        player.ShouldBeNull();
    }
}
