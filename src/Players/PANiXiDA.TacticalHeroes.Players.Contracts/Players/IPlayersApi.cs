using PANiXiDA.Core.ResultPattern;

namespace PANiXiDA.TacticalHeroes.Players.Contracts.Players;

public interface IPlayersApi
{
    Task<Result<PlayerResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
}
