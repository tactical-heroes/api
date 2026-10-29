using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.ResultPattern;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Process;

using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching.MatchmakingTestSupport;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class MatchmakingMediatorTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Matcher should persist both modes and publish contracts when command is dispatched")]
    public async Task HandleAsync_Should_PersistBothModesAndPublishContracts_When_CommandIsDispatched()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var rankedFirst = CreateSearch();
        var rankedSecond = CreateSearch(1510);
        var casualFirst = CreateSearch(100, "CasualDuel");
        var casualSecond = CreateSearch(9000, "CasualDuel");
        var outlier = CreateSearch(4000);
        await SeedAsync(fixture.Services, [rankedFirst, rankedSecond, casualFirst, casualSecond, outlier], ct);
        await using var scope = fixture.Services.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .SendAsync(new ProcessMatchmakingCommand(), ct);
        var ranked = await WaitForFoundAsync(fixture.Services, rankedFirst.Id.Value, ct);
        var casual = await WaitForFoundAsync(fixture.Services, casualFirst.Id.Value, ct);

        result.IsSuccess.ShouldBeTrue();
        ranked.Mode.ShouldBe("RankedDuel");
        casual.Mode.ShouldBe("CasualDuel");
        Guid[] ids =
        [
            rankedFirst.Id.Value,
            rankedSecond.Id.Value,
            casualFirst.Id.Value,
            casualSecond.Id.Value,
            outlier.Id.Value
        ];
        var rows = await ReadAsync(fixture.Services, ids, ct);
        rows.Count(search => search.Status == "Matched").ShouldBe(4);
        rows.Single(search => search.Id == outlier.Id.Value).Status.ShouldBe("Searching");
        rows.ShouldAllBe(search => search.GameId == null);
    }

    [Fact(DisplayName = "Confirmation should publish one pair when confirmation is repeated")]
    public async Task HandleAsync_Should_PublishOnePair_When_ConfirmationIsRepeated()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var first = CreateSearch();
        var second = CreateSearch();
        await SeedAsync(fixture.Services, [first, second], ct);
        var command = new ConfirmMatchCommand(first.Id.Value, second.Id.Value);

        var initial = await ConfirmAsync(command, ct);
        var found = await WaitForFoundAsync(fixture.Services, first.Id.Value, ct);
        var repeated = await ConfirmAsync(command, ct);

        initial.IsSuccess.ShouldBeTrue();
        repeated.IsSuccess.ShouldBeTrue();
        found.FirstPlayerId.ShouldBe(first.PlayerId.Value);
        found.SecondPlayerId.ShouldBe(second.PlayerId.Value);
        found.FirstBuildId.ShouldBe(first.BuildId.Value);
        found.SecondBuildId.ShouldBe(second.BuildId.Value);
        var published = fixture.Services.GetRequiredService<MatchFoundProbe>().Events;
        published.Count(found => found.FirstMatchSearchId == first.Id.Value).ShouldBe(1);
    }

    [Theory(DisplayName = "Confirmation should preserve searches when pair became stale")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task HandleAsync_Should_PreserveSearches_When_PairBecameStale(bool cancelled)
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var first = CreateSearch();
        var second = CreateSearch(queuedAt: cancelled ? null : DateTimeOffset.UtcNow.AddMinutes(-6));
        if (cancelled)
        {
            second.Cancel();
        }

        await SeedAsync(fixture.Services, [first, second], ct);

        var result = await ConfirmAsync(new ConfirmMatchCommand(first.Id.Value, second.Id.Value), ct);

        result.IsSuccess.ShouldBeTrue();
        var rows = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        rows.Single(search => search.Id == first.Id.Value).Status.ShouldBe("Searching");
        rows.Single(search => search.Id == second.Id.Value).Status.ShouldBe(cancelled ? "Cancelled" : "Searching");
        fixture.Services.GetRequiredService<MatchFoundProbe>().Events.ShouldNotContain(
            found => found.FirstMatchSearchId == first.Id.Value || found.SecondMatchSearchId == first.Id.Value);
    }

    [Fact(DisplayName = "Confirmation should roll back losing pair and its event when two pairs share a player")]
    public async Task HandleAsync_Should_RollBackLosingPairAndItsEvent_When_TwoPairsShareAPlayer()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var first = CreateSearch();
        var second = CreateSearch();
        var third = CreateSearch();
        await SeedAsync(fixture.Services, [first, second, third], ct);
        fixture.Services.GetRequiredService<ConcurrentMatchSaveBarrier>().Arm();

        var results = await Task.WhenAll(
            ConfirmAsync(new ConfirmMatchCommand(first.Id.Value, second.Id.Value), ct),
            ConfirmAsync(new ConfirmMatchCommand(first.Id.Value, third.Id.Value), ct));
        var found = await WaitForFoundAsync(fixture.Services, first.Id.Value, ct);

        results.Count(result => result.IsSuccess).ShouldBe(1);
        results.Single(result => result.IsFailure).Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
        var rows = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value, third.Id.Value], ct);
        rows.Count(search => search.Status == "Matched").ShouldBe(2);
        rows.Count(search => search.Status == "Searching").ShouldBe(1);
        rows.Single(search => search.Id == found.SecondMatchSearchId).Status.ShouldBe("Matched");
        var published = fixture.Services.GetRequiredService<MatchFoundProbe>().Events;
        published.Count(found => found.FirstMatchSearchId == first.Id.Value).ShouldBe(1);
    }

    private async Task<Result> ConfirmAsync(ConfirmMatchCommand command, CancellationToken ct)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>().SendAsync(command, ct);
    }
}
