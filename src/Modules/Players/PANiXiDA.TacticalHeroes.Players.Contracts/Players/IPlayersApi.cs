namespace PANiXiDA.TacticalHeroes.Modules.Players.Contracts.Players;

public interface IPlayersApi
{
    Task<PlayerResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
