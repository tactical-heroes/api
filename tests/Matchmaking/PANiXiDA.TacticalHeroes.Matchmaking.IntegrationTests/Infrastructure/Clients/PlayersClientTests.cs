using PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.IntegrationTests.Infrastructure.Clients;

public sealed class PlayersClientTests
{
    [Fact(DisplayName = "Client should map contract profile when player exists")]
    public async Task GetByUserIdAsync_Should_MapContractProfile_When_PlayerExists()
    {
        var userId = Guid.CreateVersion7();
        var profile = new PlayerProfileResponse(Guid.CreateVersion7(), 1742);
        var api = new StubPlayersApi(profile);
        var client = new PlayersClient(api);
        var ct = TestContext.Current.CancellationToken;

        var result = (await client.GetByUserIdAsync(userId, ct))!;

        result.Id.ShouldBe(profile.Id);
        result.Rating.ShouldBe(profile.Rating);
        api.UserId.ShouldBe(userId);
        (api.CancellationToken == ct).ShouldBeTrue();
    }

    [Fact(DisplayName = "Client should return null when player is missing")]
    public async Task GetByUserIdAsync_Should_ReturnNull_When_PlayerIsMissing()
    {
        var client = new PlayersClient(new StubPlayersApi(null));

        var result = await client.GetByUserIdAsync(Guid.CreateVersion7(), TestContext.Current.CancellationToken);

        result.ShouldBeNull();
    }

    private sealed class StubPlayersApi(PlayerProfileResponse? profile) : IPlayersApi
    {
        public Guid UserId { get; private set; }
        public CancellationToken CancellationToken { get; private set; }

        public Task<PlayerProfileResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
        {
            UserId = userId;
            CancellationToken = cancellationToken;

            return Task.FromResult(profile);
        }
    }
}
