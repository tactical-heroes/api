using PANiXiDA.TacticalHeroes.Modules.Battle.Application.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Modules.Battle.Infrastructure.Persistence.Features.Battles.Read;

public sealed class BattlesReadRepository(BattleReadDbContext dbContext)
    : EfReadRepository<BattleReadDbContext, Guid, BattleReadDbModel>(dbContext), IBattlesReadRepository;
