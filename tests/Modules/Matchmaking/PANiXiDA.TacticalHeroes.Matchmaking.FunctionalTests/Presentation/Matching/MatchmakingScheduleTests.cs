using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

using Wolverine.Runtime;

using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching.MatchmakingTestSupport;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

[Collection(BackgroundMatchmakingTestCollectionDefinition.Name)]
public sealed class MatchmakingScheduleTests(BackgroundMatchmakingTestFixture fixture)
{
    [Fact(DisplayName = "Schedule should target the command every five seconds when host is configured")]
    public void Scheduling_Should_TargetCommandEveryFiveSeconds_When_HostIsConfigured()
    {
        var runtime = fixture.Services.GetRequiredService<IWolverineRuntime>();

        var schedule = runtime.Options.Schedules.FindByName("matchmaking-process");

        schedule.ShouldNotBeNull();
        schedule.MessageType.ShouldBe(typeof(ProcessMatchmakingCommand));
        schedule.Schedule.Expression.ShouldBe("*/5 * * * * *");
        schedule.Schedule.TimeZone.Id.ShouldBe("UTC");
    }

    [Fact(DisplayName = "Schedule should match waiting players when recurring schedule fires")]
    public async Task Scheduling_Should_MatchWaitingPlayers_When_RecurringScheduleFires()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var first = CreateSearch();
        var second = CreateSearch();
        await SeedAsync(fixture.Services, [first, second], ct);

        var found = await WaitForFoundAsync(fixture.Services, first.Id.Value, ct);

        found.SecondMatchSearchId.ShouldBe(second.Id.Value);
        var rows = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        rows.ShouldAllBe(search => search.Status == "Matched");
    }
}
