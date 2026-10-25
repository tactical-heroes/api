using Microsoft.EntityFrameworkCore;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;

public sealed class BattleReadDbContext(
    DbContextOptions<BattleReadDbContext> options)
    : ReadDbContext<BattleReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;
}
