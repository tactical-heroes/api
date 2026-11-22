using FluentValidation;

using PANiXiDA.TacticalHeroes.Games.Application.Games.Events;
using PANiXiDA.TacticalHeroes.Games.Domain.Games;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Abstractions;
using PANiXiDA.TacticalHeroes.Games.Domain.Games.Events;

namespace PANiXiDA.TacticalHeroes.Games.UnitTests.Application.Games.Events;

public sealed class GameCreationRequestedHandlerTests
{
    [Theory(DisplayName = "Handler should create a waiting game and record correlation when match is found")]
    [InlineData("RankedDuel")]
    [InlineData("CasualDuel")]
    public async Task HandleAsync_Should_CreateWaitingGameAndRecordCorrelation_When_MatchIsFound(string mode)
    {
        var ct = TestContext.Current.CancellationToken;
        var repository = Substitute.For<IGamesRepository>();
        var handler = new GameCreationRequestedHandler(repository);
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
        var created = game.GetDomainEvents().ShouldHaveSingleItem().ShouldBeOfType<GameCreated>();
        created.GameId.ShouldBe(game.Id.Value);
        created.FirstMatchSearchId.ShouldBe(requested.FirstMatchSearchId);
        created.SecondMatchSearchId.ShouldBe(requested.SecondMatchSearchId);
    }

    [Theory(DisplayName = "Handler should reject invalid matches before persistence when match data is invalid")]
    [InlineData("UnknownMode")]
    [InlineData("EmptyPlayer")]
    [InlineData("EmptyBuild")]
    [InlineData("SamePlayer")]
    [InlineData("EmptyFirstSearch")]
    [InlineData("EmptySecondSearch")]
    [InlineData("SameSearch")]
    public async Task HandleAsync_Should_RejectInvalidMatchBeforePersistence_When_MatchDataIsInvalid(string scenario)
    {
        var repository = Substitute.For<IGamesRepository>();
        var handler = new GameCreationRequestedHandler(repository);
        var request = CreateRequest("RankedDuel");
        request = scenario switch
        {
            "UnknownMode" => request with { Mode = "Unknown" },
            "EmptyPlayer" => request with { FirstPlayerId = Guid.Empty },
            "EmptyBuild" => request with { SecondBuildId = Guid.Empty },
            "SamePlayer" => request with { SecondPlayerId = request.FirstPlayerId },
            "EmptyFirstSearch" => request with { FirstMatchSearchId = Guid.Empty },
            "EmptySecondSearch" => request with { SecondMatchSearchId = Guid.Empty },
            _ => request with { SecondMatchSearchId = request.FirstMatchSearchId }
        };

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            handler.HandleAsync(request, TestContext.Current.CancellationToken));

        exception.Errors.ShouldHaveSingleItem().ErrorMessage.ShouldNotBeNullOrWhiteSpace();
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Handler should collect validation errors when several fields are invalid")]
    public async Task HandleAsync_Should_CollectValidationErrors_When_SeveralFieldsAreInvalid()
    {
        var repository = Substitute.For<IGamesRepository>();
        var handler = new GameCreationRequestedHandler(repository);
        var request = CreateRequest("Unknown") with
        {
            FirstPlayerId = Guid.Empty,
            SecondBuildId = Guid.Empty,
            FirstMatchSearchId = Guid.Empty
        };

        var exception = await Should.ThrowAsync<ValidationException>(() =>
            handler.HandleAsync(request, TestContext.Current.CancellationToken));

        exception.Errors.Count().ShouldBe(4);
        repository.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Handler should propagate persistence failure when repository fails")]
    public async Task HandleAsync_Should_PropagatePersistenceFailure_When_RepositoryFails()
    {
        var repository = Substitute.For<IGamesRepository>();
        var ct = TestContext.Current.CancellationToken;
        var failure = new InvalidOperationException("Repository unavailable.");
        repository.When(item => item.Add(Arg.Any<Game>())).Do(_ => throw failure);
        var handler = new GameCreationRequestedHandler(repository);

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
