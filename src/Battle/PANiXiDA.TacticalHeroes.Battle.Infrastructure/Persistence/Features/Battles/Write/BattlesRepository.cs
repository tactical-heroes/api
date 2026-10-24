using PANiXiDA.TacticalHeroes.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;

using BattleAggregate = PANiXiDA.TacticalHeroes.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Write;

public sealed class BattlesRepository(BattleWriteDbContext dbContext)
    : EfRepository<BattleWriteDbContext, BattleId, BattleAggregate>(dbContext), IBattlesRepository;
