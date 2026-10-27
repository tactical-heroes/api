using PANiXiDA.TacticalHeroes.Players.Application.Players.Register;
using PANiXiDA.TacticalHeroes.Players.Domain.Players;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;
using PANiXiDA.TacticalHeroes.Players.Domain.Players.Events;

namespace PANiXiDA.TacticalHeroes.Players.UnitTests.Application.Players.Register;

public sealed class PlayerRegistrationRequestedHandlerTests
{
    [Fact(DisplayName = "Registration handler should create an initial player when registration is requested")]
    public async Task HandleAsync_Should_CreateInitialPlayer_When_RegistrationIsRequested()
    {
        var userId = Guid.CreateVersion7();
        var repository = Substitute.For<IPlayersRepository>();
        var handler = new PlayerRegistrationRequestedHandler(repository);
        var domainEvent = new PlayerRegistrationRequested(Guid.CreateVersion7(), userId);
        var ct = TestContext.Current.CancellationToken;

        await handler.HandleAsync(domainEvent, ct);

        await repository.Received(1).AddIfMissingAsync(
            Arg.Is<Player>(player => player.UserId.Value == userId
                && player.Id.Value != Guid.Empty
                && player.Rating.Value == 1000
                && player.AvatarFileId == null),
            ct);
    }

    [Fact(DisplayName = "Registration handler should reject invalid user ids when user id is empty")]
    public async Task HandleAsync_Should_RejectInvalidUserIds_When_UserIdIsEmpty()
    {
        var repository = Substitute.For<IPlayersRepository>();
        var handler = new PlayerRegistrationRequestedHandler(repository);
        var domainEvent = new PlayerRegistrationRequested(Guid.CreateVersion7(), Guid.Empty);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken));

        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Registration handler should preserve persistence failures when persistence fails")]
    public async Task HandleAsync_Should_PreservePersistenceFailures_When_PersistenceFails()
    {
        var repository = Substitute.For<IPlayersRepository>();
        var failure = new InvalidOperationException("Storage unavailable.");
        repository.AddIfMissingAsync(Arg.Any<Player>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException(failure));
        var handler = new PlayerRegistrationRequestedHandler(repository);
        var domainEvent = new PlayerRegistrationRequested(Guid.CreateVersion7(), Guid.CreateVersion7());

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.HandleAsync(domainEvent, TestContext.Current.CancellationToken));

        exception.ShouldBeSameAs(failure);
    }
}
