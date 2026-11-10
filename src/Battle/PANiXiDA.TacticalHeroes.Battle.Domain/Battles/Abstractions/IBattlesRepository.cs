using PANiXiDA.TacticalHeroes.Battle.Domain.Games;

namespace PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;

public interface IBattlesRepository : IRepository<BattleId, Battle>
{
    Task<Battle?> GetByGameIdAsync(GameId gameId, CancellationToken cancellationToken);
}
