using PANiXiDA.TacticalHeroes.Players.Application.Players.GetProfileByUserId;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;

public interface IPlayersReadRepository : IReadRepository<Guid>
{
    Task<PlayerProfileReadModel?> GetProfileByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
