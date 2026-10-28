using PANiXiDA.TacticalHeroes.Matchmaking.Application.Players;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

public sealed class PlayersClient(IPlayersApi playersApi) : IPlayersClient
{
    public async Task<MatchmakingPlayer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var profile = await playersApi.GetByUserIdAsync(userId, cancellationToken);

        return profile is null ? null : new MatchmakingPlayer(profile.Id, profile.Rating);
    }
}
