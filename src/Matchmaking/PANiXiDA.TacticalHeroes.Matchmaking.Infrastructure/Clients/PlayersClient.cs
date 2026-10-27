using PANiXiDA.TacticalHeroes.Matchmaking.Application.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Clients;

public sealed class PlayersClient : IPlayersClient
{
    public Task<MatchmakingPlayer?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Until Players is connected, use a stable mock profile for each user.
        return Task.FromResult<MatchmakingPlayer?>(new MatchmakingPlayer(userId, Rating: 1000));
    }
}
