using PANiXiDA.TacticalHeroes.Players.Application.Players.GetInternalPlayer;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;

public interface IPlayersReadRepository : IReadRepository<Guid>
{
    Task<PlayerReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
