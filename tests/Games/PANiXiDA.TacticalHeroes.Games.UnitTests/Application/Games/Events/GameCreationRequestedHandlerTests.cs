using PANiXiDA.Core.Application.Messaging.EventBus;

using PANiXiDA.TacticalHeroes.Games.Application.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Application.Games.Events;

public sealed class GameCreationRequestedHandlerTests
{
    [Theory(DisplayName = "Handler should create a waiting game and publish correlation when match is found")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public async Task HandleAsync_Should_CreateWaitingGameAndPublishCorrelation_When_MatchIsFound(string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        var repository = Substitute.For<IGamesRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var handler = new GameCreationRequestedHandler(repository, eventBus);
        var requested = CreateRequest(mode);
        Game? added = null;
        repository.When(item => item.Add(Arg.Any<Game>())).Do(call => added = call.Arg<Game>());

        await handler.HandleAsync(requested, ct);

        var game = added.ShouldNotBeNull();
        game.Mode.Name.ShouldBe(mode);
        game.Status.Name.ShouldBe("WaitingForPlayers");
        game.BattleId.ShouldBeNull();
        game.Participants.Count.ShouldBe(2);
        var first = game.Participants.Single(item => item.PlayerId.Value == requested.FirstPlayerId);
        first.BuildId.Value.ShouldBe(requested.FirstBuildId);
        first.Rating.Value.ShouldBe(requested.FirstRating);
        var second = game.Participants.Single(item => item.PlayerId.Value == requested.SecondPlayerId);
        second.BuildId.Value.ShouldBe(requested.SecondBuildId);
        second.Rating.Value.ShouldBe(requested.SecondRating);
        await eventBus.Received(1).PublishAsync(
            Arg.Is<GameCreated>(created => created.GameId == game.Id.Value
                && created.FirstMatchSearchId == requested.FirstMatchSearchId
                && created.SecondMatchSearchId == requested.SecondMatchSearchId),
            ct);
    }

    [Theory(DisplayName = "Handler should reject invalid matches before persistence when match data is invalid")]
    [InlineData("UnknownMode")]
    [InlineData("EmptyPlayer")]
    [InlineData("EmptyBuild")]
    [InlineData("SamePlayer")]
    public async Task HandleAsync_Should_RejectInvalidMatchBeforePersistence_When_MatchDataIsInvalid(string scenario)
    {
        var repository = Substitute.For<IGamesRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var handler = new GameCreationRequestedHandler(repository, eventBus);
        var request = CreateRequest("RankedDuel");
        request = scenario switch
        {
            "UnknownMode" => request with { Mode = "Unknown" },
            "EmptyPlayer" => request with { FirstPlayerId = Guid.Empty },
            "EmptyBuild" => request with { SecondBuildId = Guid.Empty },
            _ => request with { SecondPlayerId = request.FirstPlayerId }
        };

        await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.HandleAsync(request, TestContext.Current.CancellationToken));

        repository.ReceivedCalls().ShouldBeEmpty();
        eventBus.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Handler should propagate publication failure when outbox fails")]
    public async Task HandleAsync_Should_PropagatePublicationFailure_When_OutboxFails()
    {
        var repository = Substitute.For<IGamesRepository>();
        var eventBus = Substitute.For<IEventBus>();
        var ct = TestContext.Current.CancellationToken;
        var failure = new InvalidOperationException("Outbox unavailable.");
        eventBus.PublishAsync(Arg.Any<GameCreated>(), ct).Returns(Task.FromException(failure));
        var handler = new GameCreationRequestedHandler(repository, eventBus);

        var exception = await Should.ThrowAsync<InvalidOperationException>(() =>
            handler.HandleAsync(CreateRequest("RankedDuel"), ct));

        exception.ShouldBeSameAs(failure);
    }

    private static GameCreationRequested CreateRequest(string mode)
    {
        return new GameCreationRequested(
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1500,
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            Guid.CreateVersion7(),
            1510,
            mode,
            DateTimeOffset.UtcNow);
    }
}
