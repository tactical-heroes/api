using Microsoft.EntityFrameworkCore;

namespace PANiXiDA.TacticalHeroes.Modules.Matchmaking.Infrastructure.Persistence.Core;

public sealed class MatchmakingReadDbContext(
    DbContextOptions<MatchmakingReadDbContext> options)
    : ReadDbContext<MatchmakingReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;
}
