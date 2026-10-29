using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.Builds;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.ConfirmMatch;

public sealed class ConfirmMatchHandlerTests
{
    [Fact(DisplayName = "Handler should persist pair once when confirmation is repeated")]
    public async Task HandleAsync_Should_PersistPairOnce_When_ConfirmationIsRepeated()
    {
        var first = CreateSearch();
        var second = CreateSearch();
        var repository = Repository(first, second);
        var handler = Handler(repository);
        var command = new ConfirmMatchCommand(first.Id.Value, second.Id.Value);
        var ct = TestContext.Current.CancellationToken;

        var initial = await handler.HandleAsync(command, ct);
        var repeated = await handler.HandleAsync(command, ct);

        initial.IsSuccess.ShouldBeTrue();
        repeated.IsSuccess.ShouldBeTrue();
        first.Status.ShouldBe(MatchSearchStatus.Matched);
        second.Status.ShouldBe(MatchSearchStatus.Matched);
        repository.Received(1).Update(first);
        repository.Received(1).Update(second);
        first.GetDomainEvents().OfType<MatchFound>().Count().ShouldBe(1);
    }

    [Theory(DisplayName = "Handler should skip stale pair when searches changed")]
    [InlineData("Missing")]
    [InlineData("Cancelled")]
    [InlineData("Matched")]
    [InlineData("Expired")]
    [InlineData("WrongMode")]
    [InlineData("RatingChanged")]
    [InlineData("SamePlayer")]
    public async Task HandleAsync_Should_SkipStalePair_When_SearchesChanged(string scenario)
    {
        var first = CreateSearch();
        var second = CreateSearch(
            playerId: scenario == "SamePlayer" ? first.PlayerId.Value : null,
            mode: scenario == "WrongMode" ? MatchmakingMode.CasualDuel : null);
        if (scenario == "Cancelled")
        {
            second.Cancel();
        }
        else if (scenario == "Matched")
        {
            second.Matched(At(1));
        }
        else if (scenario == "Expired")
        {
            second.Expire(At(300));
        }
        else if (scenario == "RatingChanged")
        {
            second = MatchSearch.Create(
                second.PlayerId, BuildId.New(), Rating.Create(3000), MatchmakingMode.RankedDuel, At(0)).Value;
        }

        var repository = Repository(first, second);
        if (scenario == "Missing")
        {
            repository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns((MatchSearch?)null);
        }

        var handler = Handler(repository);

        var result = await handler.HandleAsync(
            new ConfirmMatchCommand(first.Id.Value, second.Id.Value), TestContext.Current.CancellationToken);

        result.IsSuccess.ShouldBeTrue();
        first.Status.ShouldBe(MatchSearchStatus.Searching);
        repository.DidNotReceiveWithAnyArgs().Update(null!);
        first.GetDomainEvents().OfType<MatchFound>().ShouldBeEmpty();
    }

    [Fact(DisplayName = "Handler should reject request when ids are invalid")]
    public async Task HandleAsync_Should_RejectRequest_When_IdsAreInvalid()
    {
        var repository = Substitute.For<IMatchSearchesRepository>();
        var handler = Handler(repository);

        var result = await handler.HandleAsync(
            new ConfirmMatchCommand(Guid.Empty, Guid.Empty), TestContext.Current.CancellationToken);

        result.IsFailure.ShouldBeTrue();
        result.Errors.Count.ShouldBe(2);
        await repository.DidNotReceiveWithAnyArgs().GetByIdAsync(default, TestContext.Current.CancellationToken);
    }

    private static IMatchSearchesRepository Repository(MatchSearch first, MatchSearch second)
    {
        var repository = Substitute.For<IMatchSearchesRepository>();
        repository.GetByIdAsync(first.Id, Arg.Any<CancellationToken>()).Returns(first);
        repository.GetByIdAsync(second.Id, Arg.Any<CancellationToken>()).Returns(second);
        return repository;
    }

    private static ConfirmMatchHandler Handler(IMatchSearchesRepository repository)
    {
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(At(10).Value);
        return new ConfirmMatchHandler(repository, clock);
    }
}
