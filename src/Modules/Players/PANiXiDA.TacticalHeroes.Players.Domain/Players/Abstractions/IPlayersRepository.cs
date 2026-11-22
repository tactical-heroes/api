using PANiXiDA.TacticalHeroes.Modules.Players.Domain.Users;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Domain.Players.Abstractions;

public interface IPlayersRepository : IRepository<PlayerId, Player>
{
    Task<Player?> GetByUserIdAsync(UserId userId, CancellationToken cancellationToken);
}
