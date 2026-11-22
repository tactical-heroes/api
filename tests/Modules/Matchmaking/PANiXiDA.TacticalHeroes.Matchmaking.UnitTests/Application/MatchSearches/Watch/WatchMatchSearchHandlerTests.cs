using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchHandlerTests
{
    [Fact(DisplayName = "Handler should subscribe before reading and sending state when search exists")]
    public async Task HandleAsync_Should_SubscribeBeforeReadingAndSendingState_When_SearchExists()
    {
        var user = Substitute.For<ICurrentUser>();
        var players = Substitute.For<IPlayersClient>();
        var notifier = Substitute.For<IMatchSearchStateNotifier>();
        var repository = Substitute.For<IMatchSearchesReadRepository>();
        var userId = Guid.NewGuid();
        var player = new PlayerReadModel(Guid.NewGuid(), 1500);
        var ct = TestContext.Current.CancellationToken;
        var current = new MatchSearchReadModel(Guid.NewGuid(), "Searching", null, null, null);
        user.UserId.Returns(userId);
        players.GetByUserIdAsync(userId, ct).Returns(player);
        repository.GetStateByPlayerIdAsync(player.Id, ct).Returns(current);
        var handler = new WatchMatchSearchHandler(players, user, notifier, repository);

        var result = await handler.HandleAsync(new WatchMatchSearchCommand("connection"), ct);

        result.IsSuccess.ShouldBeTrue();
        await notifier.Received(1).SubscribeAsync(player.Id, "connection", ct);
        await repository.Received(1).GetStateByPlayerIdAsync(player.Id, ct);
        await notifier.Received(1).SendAsync("connection", current, ct);
        await players.Received(1).GetByUserIdAsync(userId, ct);
        Received.InOrder(() =>
        {
            _ = notifier.SubscribeAsync(player.Id, "connection", ct);
            _ = repository.GetStateByPlayerIdAsync(player.Id, ct);
            _ = notifier.SendAsync("connection", current, ct);
        });
    }

    [Fact(DisplayName = "Handler should subscribe without sending state when no search exists")]
    public async Task HandleAsync_Should_SubscribeWithoutSendingState_When_NoSearchExists()
    {
        var user = Substitute.For<ICurrentUser>();
        var players = Substitute.For<IPlayersClient>();
        var notifier = Substitute.For<IMatchSearchStateNotifier>();
        var repository = Substitute.For<IMatchSearchesReadRepository>();
        var userId = Guid.NewGuid();
        var player = new PlayerReadModel(Guid.NewGuid(), 1500);
        var ct = TestContext.Current.CancellationToken;
        user.UserId.Returns(userId);
        players.GetByUserIdAsync(userId, ct).Returns(player);
        var handler = new WatchMatchSearchHandler(players, user, notifier, repository);

        var result = await handler.HandleAsync(new WatchMatchSearchCommand("connection"), ct);

        result.IsSuccess.ShouldBeTrue();
        await notifier.DidNotReceive().SendAsync(
            Arg.Any<string>(), Arg.Any<MatchSearchReadModel>(), Arg.Any<CancellationToken>());
        Received.InOrder(() =>
        {
            _ = notifier.SubscribeAsync(player.Id, "connection", ct);
            _ = repository.GetStateByPlayerIdAsync(player.Id, ct);
        });
    }

    [Theory(DisplayName = "Handler should reject subscription when player is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RejectSubscription_When_PlayerIsUnavailable(bool authenticated)
    {
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(authenticated ? Guid.NewGuid() : (Guid?)null);
        var notifier = Substitute.For<IMatchSearchStateNotifier>();
        var repository = Substitute.For<IMatchSearchesReadRepository>();
        var handler = new WatchMatchSearchHandler(Substitute.For<IPlayersClient>(), user, notifier, repository);

        var result = await handler.HandleAsync(
            new WatchMatchSearchCommand("connection"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(authenticated ? ErrorType.NotFound : ErrorType.Unauthorized);
        notifier.ReceivedCalls().ShouldBeEmpty();
        repository.ReceivedCalls().ShouldBeEmpty();
    }
}
