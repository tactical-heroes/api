using Microsoft.EntityFrameworkCore;

namespace PANiXiDA.TacticalHeroes.Games.Infrastructure.Persistence.Core;

public sealed class GamesReadDbContext(
    DbContextOptions<GamesReadDbContext> options)
    : ReadDbContext<GamesReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;
}
