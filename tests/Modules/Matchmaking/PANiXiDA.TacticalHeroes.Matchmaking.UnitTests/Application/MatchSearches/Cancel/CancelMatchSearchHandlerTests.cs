using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Cancel;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Cancel;

public sealed class CancelMatchSearchHandlerTests
{
    [Fact(DisplayName = "Handler should cancel once when owner repeats cancellation")]
    public async Task HandleAsync_Should_CancelOnce_When_OwnerRepeatsCancellation()
    {
        var context = new HandlerContext();
        var ct = TestContext.Current.CancellationToken;

        var initial = await context.Handler.HandleAsync(context.Command, ct);
        var repeated = await context.Handler.HandleAsync(context.Command, ct);

        initial.IsSuccess.ShouldBeTrue();
        repeated.IsSuccess.ShouldBeTrue();
        context.Search.Status.ShouldBe(MatchSearchStatus.Cancelled);
        context.Repository.Received(1).Update(context.Search);
        context.Search.GetDomainEvents()
            .OfType<MatchSearchStateChanged>()
            .Count(@event => @event.Status == MatchSearchStatus.Cancelled.Name)
            .ShouldBe(1);
        await context.Repository.Received(2).GetByIdAsync(context.Search.Id, ct);
        await context.Players.Received(2).GetByUserIdAsync(context.UserId, ct);
    }

    [Fact(DisplayName = "Handler should reject request when user is missing")]
    public async Task HandleAsync_Should_RejectRequest_When_UserIsMissing()
    {
        var context = new HandlerContext();
        context.CurrentUser.UserId.Returns((Guid?)null);

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Unauthorized);
        await context.Players.DidNotReceiveWithAnyArgs()
            .GetByUserIdAsync(default, TestContext.Current.CancellationToken);
        context.Repository.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Handler should reject request when search id is invalid")]
    public async Task HandleAsync_Should_RejectRequest_When_SearchIdIsInvalid()
    {
        var context = new HandlerContext();

        var result = await context.Handler.HandleAsync(
            new CancelMatchSearchCommand(Guid.Empty), TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Validation);
        await context.Players.DidNotReceiveWithAnyArgs()
            .GetByUserIdAsync(default, TestContext.Current.CancellationToken);
        context.Repository.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Handler should reject request when player is missing")]
    public async Task HandleAsync_Should_RejectRequest_When_PlayerIsMissing()
    {
        var context = new HandlerContext();
        context.Players.GetByUserIdAsync(context.UserId, Arg.Any<CancellationToken>()).Returns((PlayerReadModel?)null);

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
        await context.Repository.DidNotReceiveWithAnyArgs()
            .GetByIdAsync(default, TestContext.Current.CancellationToken);
        context.Repository.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Fact(DisplayName = "Handler should reject request when player id is invalid")]
    public async Task HandleAsync_Should_RejectRequest_When_PlayerIdIsInvalid()
    {
        var context = new HandlerContext();
        context.Players.GetByUserIdAsync(context.UserId, Arg.Any<CancellationToken>())
            .Returns(new PlayerReadModel(Guid.Empty, 1500));

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Validation);
        await context.Repository.DidNotReceiveWithAnyArgs()
            .GetByIdAsync(default, TestContext.Current.CancellationToken);
        context.Repository.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Theory(DisplayName = "Handler should hide search when search is unavailable to player")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_HideSearch_When_SearchIsUnavailableToPlayer(bool foreign)
    {
        var context = new HandlerContext();
        if (foreign)
        {
            context.Players.GetByUserIdAsync(context.UserId, Arg.Any<CancellationToken>())
                .Returns(new PlayerReadModel(Guid.CreateVersion7(), 1500));
        }
        else
        {
            context.Repository.GetByIdAsync(context.Search.Id, Arg.Any<CancellationToken>())
                .Returns((MatchSearch?)null);
        }

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.NotFound);
        context.Search.Status.ShouldBe(MatchSearchStatus.Searching);
        context.Repository.DidNotReceiveWithAnyArgs().Update(null!);
    }

    [Theory(DisplayName = "Handler should preserve search when search is no longer cancellable")]
    [InlineData("Matched")]
    [InlineData("Closed")]
    [InlineData("Expired")]
    public async Task HandleAsync_Should_PreserveSearch_When_SearchIsNoLongerCancellable(string status)
    {
        var context = new HandlerContext();
        if (status == "Expired")
        {
            context.Search.Expire(At(300)).IsSuccess.ShouldBeTrue();
        }
        else
        {
            context.Search.Matched(At(1)).IsSuccess.ShouldBeTrue();
            if (status == "Closed")
            {
                context.Search.Close().IsSuccess.ShouldBeTrue();
            }
        }
        var eventCount = context.Search.GetDomainEvents().Count;

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
        context.Search.Status.Name.ShouldBe(status);
        context.Search.GetDomainEvents().Count.ShouldBe(eventCount);
        context.Repository.DidNotReceiveWithAnyArgs().Update(null!);
    }

    private sealed class HandlerContext
    {
        public HandlerContext()
        {
            Search = CreateSearch();
            CurrentUser.UserId.Returns(UserId);
            Players.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>())
                .Returns(new PlayerReadModel(Search.PlayerId.Value, Search.Rating.Value));
            Repository.GetByIdAsync(Search.Id, Arg.Any<CancellationToken>()).Returns(Search);
            Handler = new CancelMatchSearchHandler(Repository, Players, CurrentUser);
        }

        public Guid UserId { get; } = Guid.CreateVersion7();
        public MatchSearch Search { get; }
        public CancelMatchSearchCommand Command => new(Search.Id.Value);
        public IMatchSearchesRepository Repository { get; } = Substitute.For<IMatchSearchesRepository>();
        public IPlayersClient Players { get; } = Substitute.For<IPlayersClient>();
        public ICurrentUser CurrentUser { get; } = Substitute.For<ICurrentUser>();
        public CancelMatchSearchHandler Handler { get; }
    }
}
