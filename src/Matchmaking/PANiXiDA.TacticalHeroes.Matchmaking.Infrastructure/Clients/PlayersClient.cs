using PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;
using PANiXiDA.TacticalHeroes.Matchmaking.Application.Players;
using PANiXiDA.TacticalHeroes.Players.Contracts.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

public sealed class PlayersClient(IPlayersApi playersApi) : IPlayersClient
{
    public async Task<Result<MatchmakingPlayer>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        var result = await playersApi.GetByUserIdAsync(userId, cancellationToken);

        return result.Map(player => new MatchmakingPlayer(player.Id, player.Rating));
    }
}
