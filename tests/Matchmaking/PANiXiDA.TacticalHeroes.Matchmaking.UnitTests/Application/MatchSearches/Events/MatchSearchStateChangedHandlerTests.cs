using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Events;

public sealed class MatchSearchStateChangedHandlerTests
{
    [Theory(DisplayName = "Handler should notify latest state when state changes")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_NotifyLatestState_When_StateChanges(bool exists)
    {
        var publisher = Substitute.For<IMatchSearchStatePublisher>();
        var repository = Substitute.For<IMatchSearchesReadRepository>();
        var handler = new MatchSearchStateChangedHandler(publisher, repository);
        var changed = new MatchSearchStateChanged(Guid.NewGuid(), Guid.NewGuid(), "Matched", Guid.NewGuid());
        var ct = TestContext.Current.CancellationToken;
        var current = new MatchSearchStateReadModel(10, Guid.NewGuid(), "Cancelled", null, null, null);
        repository.GetStateByPlayerIdAsync(changed.PlayerId, ct).Returns(exists ? current : null);

        await handler.HandleAsync(changed, ct);

        if (exists)
        {
            await publisher.Received(1).PublishAsync(changed.PlayerId, current, ct);
        }
        else
        {
            publisher.ReceivedCalls().ShouldBeEmpty();
        }
    }
}
