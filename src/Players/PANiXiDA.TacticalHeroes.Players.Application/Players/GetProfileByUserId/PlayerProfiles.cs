using PANiXiDA.TacticalHeroes.Players.Application.Players.Abstractions;

namespace PANiXiDA.TacticalHeroes.Players.Application.Players.GetProfileByUserId;

public sealed class PlayerProfiles(IPlayersReadRepository playersReadRepository) : IPlayerProfiles
{
    public Task<PlayerProfileReadModel?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return playersReadRepository.GetProfileByUserIdAsync(userId, cancellationToken);
    }
}
