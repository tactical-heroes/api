using PANiXiDA.TacticalHeroes.Players.Application.Players.GetProfileByUserId;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;

public interface IPlayerProfiles
{
    Task<PlayerProfileReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
