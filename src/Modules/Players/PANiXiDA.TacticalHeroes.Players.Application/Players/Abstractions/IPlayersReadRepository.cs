using PANiXiDA.TacticalHeroes.Players.Application.Players.Read.GetInternalPlayer;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;

public interface IPlayersReadRepository : IReadRepository<Guid>
{
    Task<PlayerReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
