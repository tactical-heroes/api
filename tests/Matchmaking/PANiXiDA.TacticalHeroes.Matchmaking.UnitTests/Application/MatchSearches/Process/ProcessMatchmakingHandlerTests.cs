using PANiXiDA.Core.Application.Messaging.Scheduling;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

namespace PANiXiDA.TacticalHeroes.Matchmaking.UnitTests.Application.MatchSearches.Process;

public sealed class ProcessMatchmakingHandlerTests
{
    [Theory(DisplayName = "Handler should schedule only selected pairs when queue is read")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(5)]
    public async Task HandleAsync_Should_ScheduleOnlySelectedPairs_When_QueueIsRead(int count)
    {
        var now = DateTimeOffset.UtcNow;
        var searches = Enumerable.Range(0, count)
            .Select(index => new SearchingMatchSearchReadModel(
                Guid.CreateVersion7(),
                Guid.CreateVersion7(),
                1000 + index,
                "CasualDuel",
                now.AddSeconds(index - count),
                now.AddMinutes(5)))
            .ToArray();
        var repository = Substitute.For<IMatchSearchesReadRepository>();
        var scheduler = Substitute.For<IScheduler>();
        var clock = Substitute.For<TimeProvider>();
        clock.GetUtcNow().Returns(now);
        var ct = TestContext.Current.CancellationToken;
        repository.GetSearchingAsync(now, ct).Returns(searches);
        var handler = new ProcessMatchmakingHandler(
            repository, scheduler, MatchmakingRules.Create(100, 50, TimeSpan.FromSeconds(15), 500).Value, clock);

        var result = await handler.HandleAsync(new ProcessMatchmakingCommand(), ct);

        result.IsSuccess.ShouldBeTrue();
        await repository.Received(1).GetSearchingAsync(now, ct);
        await scheduler.Received(count / 2).ScheduleAsync(Arg.Any<ConfirmMatchCommand>(), TimeSpan.Zero, ct);
        for (var index = 0; index + 1 < count; index += 2)
        {
            await scheduler.Received(1).ScheduleAsync(
                new ConfirmMatchCommand(searches[index].Id, searches[index + 1].Id), TimeSpan.Zero, ct);
        }
    }
}
