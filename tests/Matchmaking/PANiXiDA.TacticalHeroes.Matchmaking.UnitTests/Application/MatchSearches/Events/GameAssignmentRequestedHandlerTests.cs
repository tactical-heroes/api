using PANiXiDA.Core.Application.Messaging.EventBus;
using PANiXiDA.Core.Domain.DomainEvents;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Games;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Events;

public sealed class GameAssignmentRequestedHandlerTests
{
    [Fact(DisplayName = "Handler should assign both searches once when game creation is delivered twice")]
    public async Task HandleAsync_Should_AssignBothSearchesOnce_When_GameCreationIsDeliveredTwice()
    {
        var first = CreateSearch();
        var second = CreateSearch();
        first.Matched(At(1));
        second.Matched(At(1));
        first.ClearDomainEvents();
        second.ClearDomainEvents();
        var repository = Repository(first, second);
        var eventBus = Substitute.For<IEventBus>();
        List<DomainEvent> published = [];
        eventBus.PublishAsync(Arg.Do<DomainEvent>(published.Add), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);
        var handler = new GameAssignmentRequestedHandler(repository, eventBus);
        var request = new GameAssignmentRequested(Guid.CreateVersion7(), first.Id.Value, second.Id.Value);
        var ct = TestContext.Current.CancellationToken;

        await handler.HandleAsync(request, ct);
        await handler.HandleAsync(request, ct);

        first.GameId.ShouldNotBeNull().Value.ShouldBe(request.GameId);
        second.GameId.ShouldBe(first.GameId);
        repository.Received(1).Update(first);
        repository.Received(1).Update(second);
        var changes = published.OfType<MatchSearchStateChanged>().ToArray();
        changes.Length.ShouldBe(2);
        changes.ShouldAllBe(item => item.GameId == request.GameId);
        changes.Select(item => item.MatchSearchId).ShouldBe([first.Id.Value, second.Id.Value]);
        first.GetDomainEvents().ShouldBeEmpty();
        second.GetDomainEvents().ShouldBeEmpty();
    }

    [Theory(DisplayName = "Handler should preserve stored games when a conflicting assignment arrives")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreserveStoredGames_When_ConflictingAssignmentArrives(bool firstConflicts)
    {
        var first = CreateSearch();
        var second = CreateSearch();
        first.Matched(At(1));
        second.Matched(At(1));
        var original = GameId.New();
        var conflicting = firstConflicts ? first : second;
        conflicting.AssignGame(original);
        var repository = Repository(first, second);
        var handler = new GameAssignmentRequestedHandler(repository, Substitute.For<IEventBus>());
        var request = new GameAssignmentRequested(Guid.CreateVersion7(), first.Id.Value, second.Id.Value);

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, TestContext.Current.CancellationToken));

        conflicting.GameId.ShouldBe(original);
        repository.DidNotReceive().Update(Arg.Any<MatchSearch>());
    }

    [Theory(DisplayName = "Handler should reject unavailable searches when either search is missing")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RejectUnavailableSearches_When_EitherSearchIsMissing(bool firstMissing)
    {
        var first = CreateSearch();
        var second = CreateSearch();
        var repository = Substitute.For<IMatchSearchesRepository>();
        repository.GetByIdAsync(first.Id, Arg.Any<CancellationToken>()).Returns(firstMissing ? null : first);
        repository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns(firstMissing ? second : null);
        var handler = new GameAssignmentRequestedHandler(repository, Substitute.For<IEventBus>());

        await Should.ThrowAsync<InvalidOperationException>(() => handler.HandleAsync(
            new GameAssignmentRequested(Guid.CreateVersion7(), first.Id.Value, second.Id.Value),
            TestContext.Current.CancellationToken));

        repository.DidNotReceive().Update(Arg.Any<MatchSearch>());
    }

    [Fact(DisplayName = "Handler should reject a repeated search id when the pair contains one search")]
    public async Task HandleAsync_Should_RejectRepeatedSearchId_When_PairContainsOneSearch()
    {
        var repository = Substitute.For<IMatchSearchesRepository>();
        var handler = new GameAssignmentRequestedHandler(repository, Substitute.For<IEventBus>());
        var id = Guid.CreateVersion7();

        await Should.ThrowAsync<InvalidOperationException>(() => handler.HandleAsync(
            new GameAssignmentRequested(Guid.CreateVersion7(), id, id),
            TestContext.Current.CancellationToken));

        repository.ReceivedCalls().ShouldBeEmpty();
    }

    private static IMatchSearchesRepository Repository(MatchSearch first, MatchSearch second)
    {
        var repository = Substitute.For<IMatchSearchesRepository>();
        repository.GetByIdAsync(first.Id, Arg.Any<CancellationToken>()).Returns(first);
        repository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns(second);
        return repository;
    }
}
