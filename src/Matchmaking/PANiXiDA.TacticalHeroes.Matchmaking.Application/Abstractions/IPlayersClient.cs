using PANiXiDA.TacticalHeroes.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.Abstractions;

public interface IPlayersClient
{
    Task<Result<MatchmakingPlayerReadModel>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
