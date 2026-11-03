using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Watch;

public sealed class WatchMatchSearchHandlerTests
{
    [Fact(DisplayName = "Handler should subscribe to own player when user exists")]
    public async Task HandleAsync_Should_SubscribeToOwnPlayer_When_UserExists()
    {
        var user = Substitute.For<ICurrentUser>();
        var players = Substitute.For<IPlayersClient>();
        var feed = Substitute.For<IMatchSearchStateFeed>();
        var subscription = Substitute.For<IMatchSearchStateSubscription>();
        var userId = Guid.NewGuid();
        var player = new PlayerReadModel(Guid.NewGuid(), 1500);
        var ct = TestContext.Current.CancellationToken;
        user.UserId.Returns(userId);
        players.GetByUserIdAsync(userId, ct).Returns(player);
        feed.SubscribeAsync(player.Id, ct).Returns(subscription);
        var handler = new WatchMatchSearchHandler(players, user, feed);

        var result = await handler.HandleAsync(new WatchMatchSearchCommand(), ct);

        result.Value.ShouldBeSameAs(subscription);
        await feed.Received(1).SubscribeAsync(player.Id, ct);
    }

    [Theory(DisplayName = "Handler should reject subscription when player is unavailable")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RejectSubscription_When_PlayerIsUnavailable(bool authenticated)
    {
        var user = Substitute.For<ICurrentUser>();
        user.UserId.Returns(authenticated ? Guid.NewGuid() : (Guid?)null);
        var feed = Substitute.For<IMatchSearchStateFeed>();
        var handler = new WatchMatchSearchHandler(Substitute.For<IPlayersClient>(), user, feed);

        var result = await handler.HandleAsync(new WatchMatchSearchCommand(), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(authenticated ? ErrorType.NotFound : ErrorType.Unauthorized);
        await feed.DidNotReceiveWithAnyArgs().SubscribeAsync(default, TestContext.Current.CancellationToken);
    }
}
