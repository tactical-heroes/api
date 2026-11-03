using Microsoft.Extensions.DependencyInjection;

using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Streaming;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Streaming;

[Collection(IntegrationTestCollectionDefinition.Name)]
public sealed class MatchSearchStateFeedTests(IntegrationTestFixture fixture)
{
    [Fact(DisplayName = "State stream should return empty state when no search exists")]
    public async Task SubscribeAsync_Should_ReturnEmptyState_When_NoSearchExists()
    {
        await using var scope = fixture.CreateScope();
        var feed = new MatchSearchStateFeed(
            new MatchSearchConnections(),
            scope.ServiceProvider.GetRequiredService<IServiceScopeFactory>());

        await using var subscription = await feed.SubscribeAsync(Guid.NewGuid(), TestContext.Current.CancellationToken);

        subscription.Current.Version.ShouldBe(0);
        subscription.Current.IsActive.ShouldBeFalse();
    }
}
