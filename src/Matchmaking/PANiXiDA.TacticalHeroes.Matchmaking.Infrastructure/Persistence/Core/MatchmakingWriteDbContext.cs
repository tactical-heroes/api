using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Matchmaking.Infrastructure.Persistence.Core;

public sealed class MatchmakingWriteDbContext(
    DbContextOptions<MatchmakingWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<MatchmakingWriteDbContext>(options, interceptors)
{
    protected override bool UseContextNameAsSchema => true;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasSequence<long>("match_search_state_versions", "matchmaking");
    }
}
