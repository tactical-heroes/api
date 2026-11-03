using Microsoft.Extensions.DependencyInjection;

using Microsoft.Extensions.Logging.Abstractions;

using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Watch;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming;

using static PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Matchmaking.MatchmakingTestData;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Streaming;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class SubscriptionTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "State stream should recover current state when listener restarts")]
    public async Task ReadChangesAsync_Should_RecoverCurrentState_When_ListenerRestarts()
    {
        var ct = TestContext.Current.CancellationToken;
        await using var scope = fixture.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MatchmakingWriteDbContext>();
        var search = CreateSearch();
        context.Add(search);
        await context.SaveChangesAsync(ct);
        var connections = new MatchSearchConnections();
        using var firstListener = new MatchSearchStateListener(
            fixture.ConnectionString, connections, NullLogger<MatchSearchStateListener>.Instance);
        await firstListener.StartAsync(ct);
        var feed = new MatchSearchStateFeed(
            connections, scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());
        await using var subscription = await feed.SubscribeAsync(search.PlayerId.Value, ct);
        await firstListener.StopAsync(ct);
        search.Cancel().IsSuccess.ShouldBeTrue();
        context.Update(search);
        await context.SaveChangesAsync(ct);
        using var secondListener = new MatchSearchStateListener(
            fixture.ConnectionString, connections, NullLogger<MatchSearchStateListener>.Instance);
        await secondListener.StartAsync(ct);
        MatchSearchStateReadModel? observed = null;
        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(ct);
            timeout.CancelAfter(TimeSpan.FromSeconds(15));
            await using var updates = subscription.ReadChangesAsync(timeout.Token).GetAsyncEnumerator(timeout.Token);
            (await updates.MoveNextAsync()).ShouldBeTrue();
            observed = updates.Current;
        }
        finally
        {
            await secondListener.StopAsync(ct);
        }

        observed.ShouldNotBeNull().Status.ShouldBe("Cancelled");
        observed.Version.ShouldBeGreaterThan(subscription.Current.Version);
    }

    [Fact(DisplayName = "State stream should stop producing changes when disposed")]
    public async Task DisposeAsync_Should_StopProducingChanges_When_Disposed()
    {
        await using var scope = fixture.CreateScope();
        var connections = new MatchSearchConnections();
        var feed = new MatchSearchStateFeed(
            connections,
            scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());
        var playerId = Guid.NewGuid();
        await using var subscription = await feed.SubscribeAsync(playerId, TestContext.Current.CancellationToken);
        await subscription.DisposeAsync();
        connections.Refresh(playerId);
        var ct = TestContext.Current.CancellationToken;
        await using var updates = subscription.ReadChangesAsync(ct).GetAsyncEnumerator(ct);

        (await updates.MoveNextAsync()).ShouldBeFalse();
    }
}
