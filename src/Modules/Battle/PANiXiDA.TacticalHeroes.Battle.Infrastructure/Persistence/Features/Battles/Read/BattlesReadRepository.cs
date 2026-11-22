using PANiXiDA.TacticalHeroes.Battle.Application.Battles.Abstractions;
using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Read.DbModels;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Features.Battles.Read;

public sealed class BattlesReadRepository(BattleReadDbContext dbContext)
    : EfReadRepository<BattleReadDbContext, Guid, BattleReadDbModel>(dbContext), IBattlesReadRepository;
