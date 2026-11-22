using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Domain.Games;
using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Write;

public sealed class BattlesRepository(BattleWriteDbContext dbContext)
    : EfRepository<BattleWriteDbContext, BattleId, BattleAggregate>(dbContext), IBattlesRepository
{
    public Task<BattleAggregate?> GetByGameIdAsync(GameId gameId, CancellationToken cancellationToken)
    {
        return Query.SingleOrDefaultAsync(battle => battle.GameId == gameId, cancellationToken);
    }
}
