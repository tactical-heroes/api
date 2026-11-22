using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Modules.Games.Infrastructure.Persistence.Core;

public sealed class GamesWriteDbContext(
    DbContextOptions<GamesWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<GamesWriteDbContext>(options, interceptors)
{
    protected override bool UseContextNameAsSchema => true;
}
