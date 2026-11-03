using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Events;

public sealed class MatchSearchStateChangedHandlerTests
{
    [Fact(DisplayName = "Handler should notify player when state changes")]
    public async Task HandleAsync_Should_NotifyPlayer_When_StateChanges()
    {
        var publisher = Substitute.For<IMatchSearchStatePublisher>();
        var handler = new MatchSearchStateChangedHandler(publisher);
        var changed = new MatchSearchStateChanged(Guid.NewGuid(), Guid.NewGuid(), "Matched", Guid.NewGuid());
        var ct = TestContext.Current.CancellationToken;

        await handler.HandleAsync(changed, ct);

        await publisher.Received(1).PublishAsync(changed.PlayerId, ct);
    }
}
