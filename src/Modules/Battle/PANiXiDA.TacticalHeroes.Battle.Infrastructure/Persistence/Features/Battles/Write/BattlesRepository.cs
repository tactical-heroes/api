using PANiXiDA.TacticalHeroes.Modules.Battle.Domain.Battles;
using PANiXiDA.TacticalHeroes.Modules.Battle.Domain.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.Persistence.Core;

using BattleAggregate = PANiXiDA.TacticalHeroes.Modules.Battle.Domain.Battles.Battle;

namespace PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.Persistence.Features.Battles.Write;

public sealed class BattlesRepository(BattleWriteDbContext dbContext)
    : EfRepository<BattleWriteDbContext, BattleId, BattleAggregate>(dbContext), IBattlesRepository;
