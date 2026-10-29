using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Scheduling.Options.MatchmakingProcessing;

using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching.MatchmakingTestSupport;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

[Collection(BackgroundMatchmakingTestCollectionDefinition.Name)]
public sealed class MatchmakingWorkerTests(BackgroundMatchmakingTestFixture fixture)
{
    [Fact(DisplayName = "Worker should retry without overlapping passes when queue save is slow and fails")]
    public async Task Processing_Should_RetryWithoutOverlappingPasses_When_QueueSaveIsSlowAndFails()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var barrier = fixture.Services.GetRequiredService<ConcurrentMatchSaveBarrier>();
        barrier.DelayAndFailNextQueueSave();
        var first = CreateSearch();
        var second = CreateSearch();
        await SeedAsync(fixture.Services, [first, second], ct);

        var found = await WaitForFoundAsync(fixture.Services, first.Id.Value, ct);

        found.SecondMatchSearchId.ShouldBe(second.Id.Value);
        barrier.FailedQueueSaves.ShouldBe(1);
        barrier.MaximumConcurrentQueueSaves.ShouldBe(1);
    }

    [Fact(DisplayName = "Worker should use one second interval when host is configured")]
    public void Processing_Should_UseOneSecondInterval_When_HostIsConfigured()
    {
        var processing = fixture.Services.GetRequiredService<IOptions<MatchmakingProcessingOptions>>().Value;

        var worker = fixture.Services.GetServices<IHostedService>().OfType<MatchmakingWorker>().ShouldHaveSingleItem();

        worker.ShouldNotBeNull();
        processing.Enabled.ShouldBeTrue();
        processing.Interval.ShouldBe(TimeSpan.FromSeconds(1));
    }

    [Fact(DisplayName = "Worker should match waiting players when one second processing is enabled")]
    public async Task Processing_Should_MatchWaitingPlayers_When_OneSecondProcessingIsEnabled()
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
        fixture.Services.GetRequiredService<IOptions<MatchmakingProcessingOptions>>().Value.Interval
            .ShouldBe(TimeSpan.FromSeconds(1));
    }
}
