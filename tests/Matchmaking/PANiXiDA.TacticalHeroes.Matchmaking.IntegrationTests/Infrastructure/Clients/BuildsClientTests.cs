using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Clients;

public sealed class BuildsClientTests
{
    [Fact(DisplayName = "Client should return mocked ownership when selection is supplied")]
    public async Task IsOwnedByPlayerAsync_Should_ReturnMockedOwnership_When_SelectionIsSupplied()
    {
        var client = new BuildsClient();

        var result = await client.IsOwnedByPlayerAsync(
            Guid.CreateVersion7(), Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.ShouldBeTrue();
    }
}
