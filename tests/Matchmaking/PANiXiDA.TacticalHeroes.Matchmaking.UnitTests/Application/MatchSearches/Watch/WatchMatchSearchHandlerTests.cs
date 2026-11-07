using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchHandlerTests
{
    [Theory(DisplayName = "Handler should send snapshot before subscribing when user exists")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_SendSnapshotBeforeSubscribing_When_UserExists(bool changed)
    {
        var user = Substitute.For<ICurrentUser>();
        var players = Substitute.For<IPlayersClient>();
        var subscription = Substitute.For<IMatchSearchStateSubscription>();
        var repository = Substitute.For<IMatchSearchesReadRepository>();
        var userId = Guid.NewGuid();
        var player = new PlayerReadModel(Guid.NewGuid(), 1500);
        var ct = TestContext.Current.CancellationToken;
        var current = new MatchSearchReadModel(Guid.NewGuid(), "Searching", null, null, null);
        user.UserId.Returns(userId);
        players.GetByUserIdAsync(userId, ct).Returns(player);
        repository.GetStateByPlayerIdAsync(player.Id, ct)
            .Returns((MatchSearchReadModel?)null, changed ? current : null);
        var handler = new WatchMatchSearchHandler(players, user, subscription, repository);

        var result = await handler.HandleAsync(new WatchMatchSearchCommand("connection"), ct);

        result.IsSuccess.ShouldBeTrue();
        var calls = subscription.ReceivedCalls().Select(call => call.GetMethodInfo().Name).ToArray();
        calls.ShouldBe(changed ? ["SendAsync", "SubscribeAsync", "SendAsync"] : ["SendAsync", "SubscribeAsync"]);
        await subscription.Received(1).SendAsync(
            "connection", Arg.Is<MatchSearchReadModel>(state => state.Id == null), ct);
        await subscription.Received(1).SubscribeAsync(player.Id, "connection", ct);
        await players.Received(1).GetByUserIdAsync(userId, ct);
        if (changed)
        {
            await subscription.Received(1).SendAsync("connection", current, ct);
        }
    }

    [Theory(DisplayName = "Handler should reject subscription when player is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RejectSubscription_When_PlayerIsUnavailable(bool authenticated)
    {
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(authenticated ? Guid.NewGuid() : (Guid?)null);
        var subscription = Substitute.For<IMatchSearchStateSubscription>();
        var repository = Substitute.For<IMatchSearchesReadRepository>();
        var handler = new WatchMatchSearchHandler(Substitute.For<IPlayersClient>(), user, subscription, repository);

        var result = await handler.HandleAsync(
            new WatchMatchSearchCommand("connection"), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(authenticated ? ErrorType.NotFound : ErrorType.Unauthorized);
        subscription.ReceivedCalls().ShouldBeEmpty();
        repository.ReceivedCalls().ShouldBeEmpty();
    }
}
