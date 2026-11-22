using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.Core.Application.Messaging.Mediator;
using PANiXiDA.Core.Application.Persistence;
using PANiXiDA.Core.ResultPattern;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.ConfirmMatch;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Expire;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.Enumerations;
using PANiXiDA.TacticalHeroes.Matchmaking.Domain.MatchSearches.ValueObjects;
using PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration
    .MatchSearchExpirationTestSupport;
using static PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Matching.MatchmakingTestSupport;

namespace PANiXiDA.TacticalHeroes.Matchmaking.FunctionalTests.Presentation.Expiration;

[Collection(FunctionalTestCollectionDefinition.Name)]
public sealed class MatchSearchExpirationMediatorTests(FunctionalTestFixture fixture)
{
    [Fact(DisplayName = "Expiration should preserve other states when command is dispatched repeatedly")]
    public async Task HandleAsync_Should_PreserveOtherStates_When_CommandIsDispatchedRepeatedly()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var queuedAt = DateTimeOffset.UtcNow.AddMinutes(-6);
        var ranked = CreateSearch(queuedAt: queuedAt);
        var casual = CreateSearch(mode: "CasualDuel", queuedAt: queuedAt);
        var active = CreateSearch();
        var matched = CreateSearch(queuedAt: queuedAt);
        matched.Matched(MatchmakingTimestamp.Create(queuedAt.AddSeconds(1))).IsSuccess.ShouldBeTrue();
        var closed = CreateSearch(queuedAt: queuedAt);
        closed.Matched(MatchmakingTimestamp.Create(queuedAt.AddSeconds(1))).IsSuccess.ShouldBeTrue();
        closed.Close().IsSuccess.ShouldBeTrue();
        var cancelled = CreateSearch(queuedAt: queuedAt);
        cancelled.Cancel().IsSuccess.ShouldBeTrue();
        await SeedAsync(fixture.Services, [ranked, casual, active, matched, closed, cancelled], ct);

        var initial = await ExpireAsync(ct);
        await WaitForExpiredAsync(fixture.Services, ranked.Id.Value, ct);
        await WaitForExpiredAsync(fixture.Services, casual.Id.Value, ct);
        var repeated = await ExpireAsync(ct);

        initial.IsSuccess.ShouldBeTrue();
        repeated.IsSuccess.ShouldBeTrue();
        Guid[] ids =
        [
            ranked.Id.Value, casual.Id.Value, active.Id.Value,
            matched.Id.Value, closed.Id.Value, cancelled.Id.Value
        ];
        var rows = await ReadAsync(fixture.Services, ids, ct);
        rows.Count.ShouldBe(6);
        rows.Count(search => search.Status == "Expired").ShouldBe(2);
        rows.Single(search => search.Id == active.Id.Value).Status.ShouldBe("Searching");
        rows.Single(search => search.Id == matched.Id.Value).Status.ShouldBe("Matched");
        rows.Single(search => search.Id == closed.Id.Value).Status.ShouldBe("Closed");
        rows.Single(search => search.Id == cancelled.Id.Value).Status.ShouldBe("Cancelled");
        var events = fixture.Services.GetRequiredService<MatchSearchStateChangedProbe>().Events;
        events.Count(@event => @event.MatchSearchId == ranked.Id.Value && @event.Status == "Expired").ShouldBe(1);
        events.Count(@event => @event.MatchSearchId == casual.Id.Value && @event.Status == "Expired").ShouldBe(1);
    }

    [Fact(DisplayName = "Expiration should publish after commit when saving is paused")]
    public async Task HandleAsync_Should_PublishAfterCommit_When_SavingIsPaused()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var search = CreateSearch(queuedAt: DateTimeOffset.UtcNow.AddMinutes(-6));
        await SeedAsync(fixture.Services, [search], ct);
        var barrier = fixture.Services.GetRequiredService<ConcurrentMatchSaveBarrier>();
        var reached = barrier.PauseNextSave(MatchSearchStatus.Expired);

        var expiration = ExpireAsync(ct);

        try
        {
            await reached.WaitAsync(TimeSpan.FromSeconds(15), ct);
            var before = await ReadAsync(fixture.Services, [search.Id.Value], ct);
            before.ShouldHaveSingleItem().Status.ShouldBe("Searching");
            fixture.Services.GetRequiredService<MatchSearchStateChangedProbe>().Events.ShouldNotContain(
                @event => @event.MatchSearchId == search.Id.Value && @event.Status == "Expired");
        }
        finally
        {
            barrier.ResumeMatchSave();
            await expiration;
        }

        var result = await expiration;
        var expired = await WaitForExpiredAsync(fixture.Services, search.Id.Value, ct);

        result.IsSuccess.ShouldBeTrue();
        expired.PlayerId.ShouldBe(search.PlayerId.Value);
        var after = await ReadAsync(fixture.Services, [search.Id.Value], ct);
        after.ShouldHaveSingleItem().Status.ShouldBe("Expired");
    }

    [Fact(DisplayName = "Expiration should retry remaining searches when concurrent cancellation rolls back batch")]
    public async Task HandleAsync_Should_RetryRemainingSearches_When_ConcurrentCancellationRollsBackBatch()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var first = CreateSearch(queuedAt: DateTimeOffset.UtcNow.AddMinutes(-7));
        var second = CreateSearch(queuedAt: DateTimeOffset.UtcNow.AddMinutes(-6));
        await SeedAsync(fixture.Services, [first, second], ct);
        var barrier = fixture.Services.GetRequiredService<ConcurrentMatchSaveBarrier>();
        var reached = barrier.PauseNextSave(MatchSearchStatus.Expired);

        var expiration = ExpireAsync(ct);

        try
        {
            await reached.WaitAsync(TimeSpan.FromSeconds(15), ct);
            await using var scope = fixture.Services.CreateAsyncScope();
            var repository = scope.ServiceProvider.GetRequiredService<IMatchSearchesRepository>();
            var cancelled = (await repository.GetByIdAsync(first.Id, ct))!;
            cancelled.Cancel().IsSuccess.ShouldBeTrue();
            repository.Update(cancelled);
            await scope.ServiceProvider.GetRequiredKeyedService<IUnitOfWork>(typeof(MatchmakingWriteDbContext))
                .SaveChangesAsync(ct);
        }
        finally
        {
            barrier.ResumeMatchSave();
            await expiration;
        }

        var result = await expiration;
        var beforeRetry = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        result.Errors.ShouldHaveSingleItem().Type.ShouldBe(ErrorType.Conflict);
        beforeRetry.Single(search => search.Id == first.Id.Value).Status.ShouldBe("Cancelled");
        beforeRetry.Single(search => search.Id == second.Id.Value).Status.ShouldBe("Searching");
        fixture.Services.GetRequiredService<MatchSearchStateChangedProbe>().Events.ShouldNotContain(
            @event => (@event.MatchSearchId == first.Id.Value || @event.MatchSearchId == second.Id.Value)
                && @event.Status == "Expired");

        var retry = await ExpireAsync(ct);
        var expired = await WaitForExpiredAsync(fixture.Services, second.Id.Value, ct);

        retry.IsSuccess.ShouldBeTrue();
        expired.PlayerId.ShouldBe(second.PlayerId.Value);
        var afterRetry = await ReadAsync(fixture.Services, [first.Id.Value, second.Id.Value], ct);
        afterRetry.Single(search => search.Id == first.Id.Value).Status.ShouldBe("Cancelled");
        afterRetry.Single(search => search.Id == second.Id.Value).Status.ShouldBe("Expired");
    }

    [Fact(DisplayName = "Confirmation should preserve expired search when stale pair command is delivered")]
    public async Task HandleAsync_Should_PreserveExpiredSearch_When_StalePairCommandIsDelivered()
    {
        var ct = TestContext.Current.CancellationToken;
        await fixture.ResetDatabaseAsync(ct);
        var overdue = CreateSearch(queuedAt: DateTimeOffset.UtcNow.AddMinutes(-6));
        var active = CreateSearch();
        await SeedAsync(fixture.Services, [overdue, active], ct);
        await ExpireAsync(ct);
        await using var scope = fixture.Services.CreateAsyncScope();

        var result = await scope.ServiceProvider.GetRequiredService<IMediator>()
            .SendAsync(new ConfirmMatchCommand(overdue.Id.Value, active.Id.Value), ct);

        result.IsSuccess.ShouldBeTrue();
        var rows = await ReadAsync(fixture.Services, [overdue.Id.Value, active.Id.Value], ct);
        rows.Single(search => search.Id == overdue.Id.Value).Status.ShouldBe("Expired");
        rows.Single(search => search.Id == active.Id.Value).Status.ShouldBe("Searching");
        fixture.Services.GetRequiredService<MatchFoundProbe>().Events.ShouldNotContain(
            found => found.FirstMatchSearchId == overdue.Id.Value || found.SecondMatchSearchId == overdue.Id.Value);
    }

    private async Task<Result> ExpireAsync(CancellationToken ct)
    {
        await using var scope = fixture.Services.CreateAsyncScope();
        return await scope.ServiceProvider.GetRequiredService<IMediator>()
            .SendAsync(new ExpireMatchSearchesCommand(), ct);
    }
}
