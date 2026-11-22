using PANiXiDA.TacticalHeroes.Players.Application.Players.Events;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Events;
using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.UnitTests.Application.Players.Events;

public sealed class PlayerRegistrationRequestedHandlerTests
{
    [Fact(DisplayName = "Registration handler should create an initial player when registration is requested")]
    public async Task HandleAsync_Should_CreateInitialPlayer_When_RegistrationIsRequested()
    {
        var userId = Guid.CreateVersion7();
        var repository = Substitute.For<IPlayersRepository>();
        var handler = new PlayerRegistrationRequestedHandler(repository);
        var domainEvent = new PlayerRegistrationRequested(userId);
        var ct = TestContext.Current.CancellationToken;

        await handler.HandleAsync(domainEvent, ct);

        await repository.Received(1).GetByUserIdAsync(Arg.Is<UserId>(id => id.Value == userId), ct);
        repository.Received(1).Add(
            Arg.Is<Player>(player => player.UserId.Value == userId
                && player.Id.Value != Guid.Empty
                && player.Rating.Value == 1000
                && player.AvatarFileId == null));
    }

    [Fact(DisplayName = "Registration handler should preserve the profile when user already has a player")]
    public async Task HandleAsync_Should_PreserveProfile_When_UserAlreadyHasAPlayer()
    {
        var player = Player.Create(UserId.New());
        var repository = Substitute.For<IPlayersRepository>();
        repository.GetByUserIdAsync(player.UserId, Arg.Any<CancellationToken>()).Returns(player);
        var handler = new PlayerRegistrationRequestedHandler(repository);
        var domainEvent = new PlayerRegistrationRequested(player.UserId.Value);
        var ct = TestContext.Current.CancellationToken;

        await handler.HandleAsync(domainEvent, ct);

        await repository.Received(1).GetByUserIdAsync(player.UserId, ct);
        repository.DidNotReceiveWithAnyArgs().Add(null!);
        repository.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Registration handler should reject invalid user ids when user id is empty")]
    public async Task HandleAsync_Should_RejectInvalidUserIds_When_UserIdIsEmpty()
    {
        var repository = Substitute.For<IPlayersRepository>();
        var handler = new PlayerRegistrationRequestedHandler(repository);
        var domainEvent = new PlayerRegistrationRequested(Guid.Empty);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken));

        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Registration handler should preserve persistence failures when persistence fails")]
    public async Task HandleAsync_Should_PreservePersistenceFailures_When_PersistenceFails()
    {
        var repository = Substitute.For<IPlayersRepository>();
        var failure = new InvalidOperationException("Storage unavailable.");
        repository.GetByUserIdAsync(Arg.Any<UserId>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Player?>(failure));
        var handler = new PlayerRegistrationRequestedHandler(repository);
        var domainEvent = new PlayerRegistrationRequested(Guid.CreateVersion7());

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken));

        exception.ShouldBeSameAs(failure);
    }
}
