using PANiXiDA.TacticalHeroes.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Players.Domain.Players.Abstractions;

public interface IPlayersRepository : IRepository<PlayerId, Player>
{
    Task<Player?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);
}
