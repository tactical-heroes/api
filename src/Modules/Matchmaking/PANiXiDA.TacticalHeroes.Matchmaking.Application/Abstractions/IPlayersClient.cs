using PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.ReadModels;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Application.Abstractions;

public interface IPlayersClient
{
    Task<PlayerReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
