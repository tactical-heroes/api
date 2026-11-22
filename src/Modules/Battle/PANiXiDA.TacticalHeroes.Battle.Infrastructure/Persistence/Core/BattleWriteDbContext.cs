using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Battle.Infrastructure.Persistence.Core;

public sealed class BattleWriteDbContext(
    DbContextOptions<BattleWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<BattleWriteDbContext>(options, interceptors)
{
    protected override bool UseContextNameAsSchema => true;
}
