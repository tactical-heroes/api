using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

using Npgsql;

using PANiXiDA.Core.Application.Persistence.Errors;

using PANiXiDA.TacticalHeroes.Players.Domain.Files;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.ValueObjects;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Features.Players.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Players.IntegrationTests.Infrastructure.Persistence.Features.Players.Write;

public sealed class PlayersRepositoryTests(IntegrationTestFixture fixture)
    : IntegrationTestBase(fixture)
{
    [Fact(DisplayName = "Player should round trip its initial profile when saved")]
    public async Task GetByIdAsync_Should_RestoreInitialProfile_When_Saved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var player = CreatePlayer();
        await AddPlayersAsync([player], cancellationToken);

        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPlayersRepository>();
        var restored = await repository.GetByIdAsync(player.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.Id.ShouldBe(player.Id);
        restored.UserId.ShouldBe(player.UserId);
        restored.AvatarFileId.ShouldBeNull();
        restored.Rating.Value.ShouldBe(1000);
    }

    [Fact(DisplayName = "Player should preserve changed values when profile and rating are saved")]
    public async Task Update_Should_PersistChangedValues_When_ProfileAndRatingAreSaved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var player = CreatePlayer();
        var avatarFileId = FileId.New();
        await AddPlayersAsync([player], cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            var loaded = (await repository.GetByIdAsync(player.Id, cancellationToken))!;
            loaded.ChangeAvatar(avatarFileId);
            loaded.ChangeRating(Rating.Create(-100));

            repository.Update(loaded);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IPlayersRepository>()
            .GetByIdAsync(player.Id, cancellationToken);
        var readContext = verificationScope.ServiceProvider.GetRequiredService<PlayersReadDbContext>();
        var readModel = await readContext.Set<PlayerReadDbModel>()
            .SingleAsync(item => item.Id == player.Id.Value, cancellationToken);

        restored.ShouldNotBeNull();
        restored.UserId.ShouldBe(player.UserId);
        restored.AvatarFileId.ShouldBe(avatarFileId);
        restored.Rating.Value.ShouldBe(-100);
        readModel.UserId.ShouldBe(player.UserId.Value);
        readModel.AvatarFileId.ShouldBe(avatarFileId.Value);
        readModel.Rating.ShouldBe(-100);
    }

    [Fact(DisplayName = "Player should persist an empty avatar when avatar is removed")]
    public async Task Update_Should_PersistEmptyAvatar_When_AvatarIsRemoved()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var player = CreatePlayer();
        player.ChangeAvatar(FileId.New());
        await AddPlayersAsync([player], cancellationToken);

        await using (var scope = Fixture.CreateScope())
        {
            var repository = scope.ServiceProvider.GetRequiredService<IPlayersRepository>();
            var loaded = (await repository.GetByIdAsync(player.Id, cancellationToken))!;
            loaded.RemoveAvatar();

            repository.Update(loaded);
            await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
        }

        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IPlayersRepository>()
            .GetByIdAsync(player.Id, cancellationToken);

        restored.ShouldNotBeNull();
        restored.AvatarFileId.ShouldBeNull();
    }

    [Fact(DisplayName = "Player persistence should reject another profile when user already has a player")]
    public async Task Add_Should_RejectAnotherProfile_When_UserAlreadyHasAPlayer()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var first = CreatePlayer();
        await AddPlayersAsync([first], cancellationToken);
        var second = Player.Create(first.UserId);
        await using var scope = Fixture.CreateScope();
        scope.ServiceProvider.GetRequiredService<IPlayersRepository>().Add(second);
        var context = scope.ServiceProvider.GetRequiredService<PlayersWriteDbContext>();

        var exception = await Should.ThrowAsync<DbUpdateException>(
            () => context.SaveChangesAsync(cancellationToken));

        var postgresException = exception.InnerException.ShouldBeOfType<PostgresException>();
        postgresException.SqlState.ShouldBe(PostgresErrorCodes.UniqueViolation);
        postgresException.ConstraintName.ShouldBe("ix_players_user_id");
    }

    [Fact(DisplayName = "Player persistence should reject a stale profile when a new rating was saved first")]
    public async Task Update_Should_RejectStaleProfile_When_ANewRatingWasSavedFirst()
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var player = CreatePlayer();
        await AddPlayersAsync([player], cancellationToken);
        await using var ratingScope = Fixture.CreateScope();
        await using var profileScope = Fixture.CreateScope();
        var ratingRepository = ratingScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
        var profileRepository = profileScope.ServiceProvider.GetRequiredService<IPlayersRepository>();
        var ratingPlayer = (await ratingRepository.GetByIdAsync(player.Id, cancellationToken))!;
        var profilePlayer = (await profileRepository.GetByIdAsync(player.Id, cancellationToken))!;
        ratingPlayer.ChangeRating(Rating.Create(1400));
        ratingRepository.Update(ratingPlayer);
        await GetUnitOfWork(ratingScope.ServiceProvider).SaveChangesAsync(cancellationToken);
        profilePlayer.ChangeAvatar(FileId.New());
        profileRepository.Update(profilePlayer);

        var exception = await Should.ThrowAsync<ConcurrencyException>(
            () => GetUnitOfWork(profileScope.ServiceProvider).SaveChangesAsync(cancellationToken));

        exception.InnerException.ShouldBeOfType<DbUpdateConcurrencyException>();
        await using var verificationScope = Fixture.CreateScope();
        var restored = await verificationScope.ServiceProvider.GetRequiredService<IPlayersRepository>()
            .GetByIdAsync(player.Id, cancellationToken);
        restored.ShouldNotBeNull();
        restored.Rating.Value.ShouldBe(1400);
        restored.AvatarFileId.ShouldBeNull();
    }

    private static Player CreatePlayer()
    {
        return Player.Create(UserId.New());
    }

    private static IUnitOfWork GetUnitOfWork(IServiceProvider services)
    {
        return services.GetRequiredKeyedService<IUnitOfWork>(typeof(PlayersWriteDbContext));
    }

    private async Task AddPlayersAsync(Player[] players, CancellationToken cancellationToken)
    {
        await using var scope = Fixture.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IPlayersRepository>();
        foreach (var player in players)
        {
            repository.Add(player);
        }

        await GetUnitOfWork(scope.ServiceProvider).SaveChangesAsync(cancellationToken);
    }
}
