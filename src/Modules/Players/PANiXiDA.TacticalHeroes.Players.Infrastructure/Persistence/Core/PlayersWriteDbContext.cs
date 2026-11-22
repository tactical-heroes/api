using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Players.Infrastructure.Persistence.Core;

public sealed class PlayersWriteDbContext(
    DbContextOptions<PlayersWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<PlayersWriteDbContext>(options, interceptors)
{
    protected override bool UseContextNameAsSchema => true;
}
