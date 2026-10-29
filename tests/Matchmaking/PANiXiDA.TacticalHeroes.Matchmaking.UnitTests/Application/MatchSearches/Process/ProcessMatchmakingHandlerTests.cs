using PANiXiDA.Core.Application.Messaging.Scheduling;
using PANiXiDA.Core.SpecificationPattern.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Specifications;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

using static PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Process;

public sealed class ProcessMatchmakingHandlerTests
{
    [Theory(DisplayName = "Handler should schedule only selected pairs when queue is read")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task HandleAsync_Should_ScheduleOnlySelectedPairs_When_QueueIsRead(int count)
    {
        var now = At(10).Value;
        var searches = Enumerable.Range(0, count)
            .Select(index => CreateSearch(mode: MatchmakingMode.CasualDuel, queuedAt: index))
            .ToArray();
        var repository = Substitute.For<IMatchSearchesRepository>();
        var scheduler = Substitute.For<IScheduler>();
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(now);
        var ct = TestContext.Current.CancellationToken;
        repository.GetAllAsync(Arg.Any<ISpecification<MatchSearch>>(), ct).Returns(searches);
        var active = CreateSearch(queuedAt: 10);
        var future = CreateSearch(queuedAt: 11);
        var handler = new ProcessMatchmakingHandler(
            repository, scheduler, MatchmakingRules.Create(100, 50, TimeSpan.FromSeconds(15), 500).Value, clock);

        var result = await handler.HandleAsync(new ProcessMatchmakingCommand(), ct);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).GetAllAsync(
            Arg.Is<ISpecification<MatchSearch>>(specification =>
                specification is SearchingMatchSearchSpecification
                    && specification.IsSatisfiedBy(active)
                    && !specification.IsSatisfiedBy(future)),
            ct);
        await scheduler.Received(count / 2).ScheduleAsync(Arg.Any<ConfirmMatchCommand>(), TimeSpan.Zero, ct);
        for (var index = 0; index + 1 < count; index += 2)
        {
            await scheduler.Received(1).ScheduleAsync(
                new ConfirmMatchCommand(searches[index].Id.Value, searches[index + 1].Id.Value), TimeSpan.Zero, ct);
        }
    }
}
