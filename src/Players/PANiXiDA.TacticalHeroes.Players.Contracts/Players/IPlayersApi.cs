namespace PANiXiDA.TacticalHeroes.Players.Contracts.Players;

public interface IPlayersApi
{
    Task<PlayerProfileResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
