using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Clients;

public sealed class PlayersClientTests
{
    [Fact(DisplayName = "Client should return stable mock profile when user is supplied")]
    public async Task GetByUserIdAsync_Should_ReturnStableMockProfile_When_UserIsSupplied()
    {
        var client = new PlayersClient();
        var userId = Guid.CreateVersion7();
        var ct = TestContext.Current.CancellationToken;

        var first = (await client.GetByUserIdAsync(userId, ct))!;
        var repeated = await client.GetByUserIdAsync(userId, ct);
        var another = (await client.GetByUserIdAsync(Guid.CreateVersion7(), ct))!;

        repeated.ShouldBe(first);
        first.Id.ShouldBe(userId);
        first.Rating.ShouldBe(1000);
        another.Id.ShouldNotBe(first.Id);
    }
}
