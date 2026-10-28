using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Persistence;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Start;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Players;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Start;

public sealed class StartMatchSearchHandlerTests
{
    [Fact(DisplayName = "Handler should use authenticated player and server rating when request is valid")]
    public async Task HandleAsync_Should_UseAuthenticatedPlayerAndServerRating_When_RequestIsValid()
    {
        var context = new HandlerContext();
        var cancellationToken = TestContext.Current.CancellationToken;

        var result = await context.Handler.HandleAsync(context.Command, cancellationToken);

        result.IsSuccess.ShouldBeTrue();
        context.Repository.Received(1).Add(
            Arg.Is<MatchSearch>(search =>
                search.Status == MatchSearchStatus.Searching
                    && search.QueuedAt.Value == At(300).Value
                    && search.ExpiresAt.Value == At(600).Value
                    && search.PlayerId.Value == context.Player.Id
                    && search.BuildId.Value == context.Command.BuildId
                    && search.Rating.Value == 1742
                    && search.GameId == null
                    && search.Mode == MatchmakingMode.RankedDuel));
        await context.Players.Received(1).GetByUserIdAsync(context.UserId, cancellationToken);
        await context.Builds.Received(1).IsOwnedByPlayerAsync(
            context.Command.BuildId, context.Player.Id, cancellationToken);
    }

    [Fact(DisplayName = "Handler should reject request when user id is missing")]
    public async Task HandleAsync_Should_RejectRequest_When_UserIdIsMissing()
    {
        var context = new HandlerContext();
        context.CurrentUser.UserId.Returns((Guid?)null);

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        await context.Players.DidNotReceiveWithAnyArgs()
            .GetByUserIdAsync(default, TestContext.Current.CancellationToken);
        context.Repository.DidNotReceiveWithAnyArgs().Add(null!);
    }

    [Fact(DisplayName = "Handler should reject request when command is invalid")]
    public async Task HandleAsync_Should_RejectRequest_When_CommandIsInvalid()
    {
        var context = new HandlerContext();

        var result = await context.Handler.HandleAsync(
            new StartMatchSearchCommand(Guid.Empty, "Unknown"), TestContext.Current.CancellationToken);

        result.Errors.Count.ShouldBe(2);
        await context.Players.DidNotReceiveWithAnyArgs()
            .GetByUserIdAsync(default, TestContext.Current.CancellationToken);
        context.Repository.DidNotReceiveWithAnyArgs().Add(null!);
    }

    [Fact(DisplayName = "Handler should reject request when player does not exist")]
    public async Task HandleAsync_Should_RejectRequest_When_PlayerDoesNotExist()
    {
        var context = new HandlerContext();
        context.Players.GetByUserIdAsync(context.UserId, Arg.Any<CancellationToken>())
            .Returns(Result.Failure<MatchmakingPlayerReadModel>(Error.NotFound("Player was not found.")));

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        await context.Builds.DidNotReceiveWithAnyArgs()
            .IsOwnedByPlayerAsync(default, default, TestContext.Current.CancellationToken);
        context.Repository.DidNotReceiveWithAnyArgs().Add(null!);
    }

    [Fact(DisplayName = "Handler should reject request when build is unavailable")]
    public async Task HandleAsync_Should_RejectRequest_When_BuildIsUnavailable()
    {
        var context = new HandlerContext();
        context.Builds.IsOwnedByPlayerAsync(context.Command.BuildId, context.Player.Id, Arg.Any<CancellationToken>())
            .Returns(false);

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        await context.Repository.DidNotReceiveWithAnyArgs()
            .GetActiveByPlayerIdForUpdateAsync(default, TestContext.Current.CancellationToken);
        context.Repository.DidNotReceiveWithAnyArgs().Add(null!);
    }

    [Theory(DisplayName = "Handler should reject request when player is occupied")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_RejectRequest_When_PlayerIsOccupied(bool matched)
    {
        var context = new HandlerContext();
        var search = CreateSearch(context.Player.Id, queuedAt: 1);
        if (matched)
        {
            search.Matched(At(2)).IsSuccess.ShouldBeTrue();
        }

        context.Repository.GetActiveByPlayerIdForUpdateAsync(
            PlayerId.Create(context.Player.Id).Value, Arg.Any<CancellationToken>())
            .Returns(search);

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        search.Status.ShouldBe(matched ? MatchSearchStatus.Matched : MatchSearchStatus.Searching);
        context.Repository.DidNotReceiveWithAnyArgs().Add(null!);
        await context.UnitOfWork.DidNotReceiveWithAnyArgs().SaveChangesAsync(TestContext.Current.CancellationToken);
    }

    [Fact(DisplayName = "Handler should expire previous search before adding replacement when search reached deadline")]
    public async Task HandleAsync_Should_ExpirePreviousSearchBeforeAddingReplacement_When_SearchReachedDeadline()
    {
        var context = new HandlerContext();
        var search = CreateSearch(context.Player.Id);
        context.Repository.GetActiveByPlayerIdForUpdateAsync(
            PlayerId.Create(context.Player.Id).Value, Arg.Any<CancellationToken>())
            .Returns(search);

        var result = await context.Handler.HandleAsync(context.Command, TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        search.Status.ShouldBe(MatchSearchStatus.Expired);
        Received.InOrder(() =>
        {
            context.Repository.Update(search);
            context.UnitOfWork.SaveChangesAsync(TestContext.Current.CancellationToken);
            context.Repository.Add(Arg.Is<MatchSearch>(newSearch =>
                newSearch.Id != search.Id && newSearch.Status == MatchSearchStatus.Searching));
        });
    }

    private sealed class HandlerContext
    {
        public HandlerContext()
        {
            CurrentUser.UserId.Returns(UserId);
            Players.GetByUserIdAsync(UserId, Arg.Any<CancellationToken>()).Returns(Result.Success(Player));
            Builds.IsOwnedByPlayerAsync(Command.BuildId, Player.Id, Arg.Any<CancellationToken>()).Returns(true);
            var clock = Substitute.For<TimeProvider>();
            clock.GetUtcNow().Returns(At(300).Value);
            Handler = new StartMatchSearchHandler(Repository, Players, Builds, CurrentUser, UnitOfWork, clock);
        }

        public Guid UserId { get; } = Guid.CreateVersion7();
        public MatchmakingPlayerReadModel Player { get; } = new(Guid.CreateVersion7(), 1742);
        public StartMatchSearchCommand Command { get; } = new(Guid.CreateVersion7(), "RankedDuel");
        public IMatchSearchesRepository Repository { get; } = Substitute.For<IMatchSearchesRepository>();
        public IPlayersClient Players { get; } = Substitute.For<IPlayersClient>();
        public IBuildsClient Builds { get; } = Substitute.For<IBuildsClient>();
        public ICurrentUser CurrentUser { get; } = Substitute.For<ICurrentUser>();
        public IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();
        public StartMatchSearchHandler Handler { get; }
    }
}
