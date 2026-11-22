using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Clients;

public sealed class PlayersClientTests
{
    [Fact(DisplayName = "Client should map contract player when player exists")]
    public async Task GetByUserIdAsync_Should_MapContractPlayer_When_PlayerExists()
    {
        var userId = Guid.CreateVersion7();
        var player = new PlayerResponse(Guid.CreateVersion7(), 1742);
        var api = new StubPlayersApi(player);
        var client = new PlayersClient(api);
        var ct = TestContext.Current.CancellationToken;

        var result = await client.GetByUserIdAsync(userId, ct);

        result.ShouldNotBeNull();
        result.Id.ShouldBe(player.Id);
        result.Rating.ShouldBe(player.Rating);
        api.UserId.ShouldBe(userId);
        (api.CancellationToken == ct).ShouldBeTrue();
    }

    [Fact(DisplayName = "Client should return null when module returns no player")]
    public async Task GetByUserIdAsync_Should_ReturnNull_When_ModuleReturnsNoPlayer()
    {
        var api = new StubPlayersApi(null);
        var client = new PlayersClient(api);

        var result = await client.GetByUserIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    private sealed class StubPlayersApi(PlayerResponse? result) : IPlayersApi
    {
        public Guid UserId { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<PlayerResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            UserId = userId;
            CancellationToken = cancellationToken;

            return Task.FromResult(result);
        }
    }
}
