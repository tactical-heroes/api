using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Persistence.Core;

public sealed class MatchmakingWriteDbContext(
    DbContextOptions<MatchmakingWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<MatchmakingWriteDbContext>(options, interceptors)
{
    protected override bool UseContextNameAsSchema => true;
}
