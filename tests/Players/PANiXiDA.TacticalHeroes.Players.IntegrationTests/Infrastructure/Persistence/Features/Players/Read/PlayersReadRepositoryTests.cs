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

    [Fact(DisplayName = "Read repository should return matching profile when user has a player")]
    public async Task GetProfileByUserIdAsync_Should_ReturnMatchingProfile_When_UserHasAPlayer()
    {
        var ct = TestContext.Current.CancellationToken;
        var player = Player.Create(UserId.New());
        player.ChangeRating(Rating.Create(1742));
        await using (var writeScope = Fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            repository.Add(Player.Create(UserId.New()));
            repository.Add(player);
            await writeScope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext))
                .SaveChangesAsync(ct);
        }

        await using var scope = Fixture.CreateScope();
        var profile = (await scope.ServiceProvider.GetRequiredService<IPlayersReadRepository>()
            .GetProfileByUserIdAsync(player.UserId.Value, ct))!;

        profile.Id.ShouldBe(player.Id.Value);
        profile.Rating.ShouldBe(1742);
    }

    [Fact(DisplayName = "Read repository should return null when user has no player")]
    public async Task GetProfileByUserIdAsync_Should_ReturnNull_When_UserHasNoPlayer()
    {
        await using var scope = Fixture.CreateScope();

        var profile = await scope.ServiceProvider.GetRequiredService<IPlayersReadRepository>()
            .GetProfileByUserIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        profile.ShouldBeNull();
    }

    [Fact(DisplayName = "Read repository should return null when player was deleted")]
    public async Task GetProfileByUserIdAsync_Should_ReturnNull_When_PlayerWasDeleted()
    {
        var ct = TestContext.Current.CancellationToken;
        var player = Player.Create(UserId.New());
        await using (var writeScope = Fixture.CreateScope())
        {
            var repository = writeScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            var unitOfWork = writeScope.ServiceProvider
                .GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext));
            repository.Add(player);
            await unitOfWork.SaveChangesAsync(ct);
            repository.Delete(player);
            await unitOfWork.SaveChangesAsync(ct);
        }

        await using var scope = Fixture.CreateScope();
        var profile = await scope.ServiceProvider.GetRequiredService<IPlayersReadRepository>()
            .GetProfileByUserIdAsync(player.UserId.Value, ct);

        profile.ShouldBeNull();
    }
}
