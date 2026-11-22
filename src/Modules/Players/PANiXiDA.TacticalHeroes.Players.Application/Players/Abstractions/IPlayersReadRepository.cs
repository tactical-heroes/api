using PANiXiDA.TacticalHeroes.Modules.Players.Application.Players.GetInternalPlayer;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Application.Players.Abstractions;

public interface IPlayersReadRepository : IReadRepository<Guid>
{
    Task<PlayerReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
