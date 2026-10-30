using PANiXiDA.Core.SpecificationPattern.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Expire;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Events;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Specifications;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Expire;

public sealed class ExpireMatchSearchesHandlerTests
{
    [Theory(DisplayName = "Handler should expire selected searches when expiration tick is processed")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(3)]
    public async Task HandleAsync_Should_ExpireSelectedSearches_When_ExpirationTickIsProcessed(int count)
    {
        var searches = Enumerable.Range(0, count).Select(index => CreateSearch(queuedAt: -index)).ToList();
        var repository = Substitute.For<IMatchSearchesRepository>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(At(300).Value);
        var ct = TestContext.Current.CancellationToken;
        repository.GetAllAsync(Arg.Any<ISpecification<MatchSearch>>(), ct).Returns(searches);
        var handler = new ExpireMatchSearchesHandler(repository, timeProvider);
        var overdue = CreateSearch();
        var active = CreateSearch(queuedAt: 1);

        var result = await handler.HandleAsync(new ExpireMatchSearchesCommand(), ct);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).GetAllAsync(
            Arg.Is<ISpecification<MatchSearch>>(specification =>
                specification is OverdueMatchSearchSpecification
                    && specification.IsSatisfiedBy(overdue)
                    && !specification.IsSatisfiedBy(active)),
            ct);
        repository.Received(count).Update(Arg.Any<MatchSearch>());
        foreach (var search in searches)
        {
            search.Status.ShouldBe(MatchSearchStatus.Expired);
            repository.Received(1).Update(search);
            var events = search.GetDomainEvents().OfType<MatchSearchStateChanged>();
            events.Count(@event => @event.Status == "Expired").ShouldBe(1);
        }
    }

    [Fact(DisplayName = "Handler should return domain failure when selected search cannot expire")]
    public async Task HandleAsync_Should_ReturnDomainFailure_When_SelectedSearchCannotExpire()
    {
        var search = CreateSearch();
        search.Matched(At(1)).IsSuccess.ShouldBeTrue();
        var repository = Substitute.For<IMatchSearchesRepository>();
        var timeProvider = Substitute.For<TimeProvider>();
        timeProvider.GetUtcNow().Returns(At(300).Value);
        var ct = TestContext.Current.CancellationToken;
        repository.GetAllAsync(Arg.Any<ISpecification<MatchSearch>>(), ct).Returns([search]);
        var handler = new ExpireMatchSearchesHandler(repository, timeProvider);

        var result = await handler.HandleAsync(new ExpireMatchSearchesCommand(), ct);

        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
        search.Status.ShouldBe(MatchSearchStatus.Matched);
        repository.DidNotReceive().Update(Arg.Any<MatchSearch>());
    }
}
