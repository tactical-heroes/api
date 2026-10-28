namespace PANiXiDA.TacticalHeroes.Players.Contracts.Players;

public interface IPlayersApi
{
    Task<PlayerResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
