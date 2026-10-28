using PANiXiDA.TacticalHeroes.Matchmaking.Application.Players;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IPlayersClient
{
    Task<Result<MatchmakingPlayer>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
