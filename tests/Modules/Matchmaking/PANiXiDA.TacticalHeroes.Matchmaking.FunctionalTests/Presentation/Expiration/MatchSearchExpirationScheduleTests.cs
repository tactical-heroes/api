using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Expire;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;

using Wolverine.Runtime;

using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration
    .MatchSearchExpirationTestSupport;
using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching.MatchmakingTestSupport;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration;

[Collection(BackgroundExpirationTestCollectionDefinition.Name)]
public sealed class MatchSearchExpirationScheduleTests(BackgroundExpirationTestFixture fixture)
{
    [Fact(DisplayName = "Schedule should expire every five seconds when matching schedule is disabled")]
    public void Scheduling_Should_ExpireEveryFiveSeconds_When_MatchingScheduleIsDisabled()
    {
        var runtime = fixture.Services.GetRequiredService<IWolverineRuntime>();

        var schedule = runtime.Options.Schedules.FindByName("matchmaking-expire-searches");

        schedule.ShouldNotBeNull();
        schedule.MessageType.ShouldBe(typeof(ExpireMatchSearchesCommand));
        schedule.Schedule.Expression.ShouldBe("*/5 * * * * *");
        schedule.Schedule.TimeZone.Id.ShouldBe("UTC");
        runtime.Options.Schedules.FindByName("matchmaking-process").ShouldBeNull();
    }

    [Fact(DisplayName = "Schedule should release player for another search when overdue search expires")]
    public async Task Scheduling_Should_ReleasePlayerForAnotherSearch_When_OverdueSearchExpires()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var previous = CreateSearch(queuedAt: DateTimeOffset.UtcNow.AddMinutes(-6));
        var active = CreateSearch();
        await SeedAsync(fixture.Services, [previous, active], ct);

        var expired = await WaitForExpiredAsync(fixture.Services, previous.Id.Value, ct);
        var next = MatchSearch.Create(
            previous.PlayerId,
            previous.BuildId,
            previous.Rating,
            previous.Mode,
            MatchmakingTimestamp.Create(DateTimeOffset.UtcNow)).Value;
        await SeedAsync(fixture.Services, [next], ct);

        expired.PlayerId.ShouldBe(previous.PlayerId.Value);
        var rows = await ReadAsync(fixture.Services, [previous.Id.Value, next.Id.Value, active.Id.Value], ct);
        rows.Count.ShouldBe(3);
        rows.Single(search => search.Id == previous.Id.Value).Status.ShouldBe("Expired");
        rows.Single(search => search.Id == next.Id.Value).Status.ShouldBe("Searching");
        rows.Single(search => search.Id == active.Id.Value).Status.ShouldBe("Searching");
    }
}
