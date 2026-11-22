using PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Common.ReadModels;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Application.MatchSearches.Abstractions;

public interface IPlayersClient
{
    Task<PlayerReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
