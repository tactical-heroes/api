using Microsoft.EntityFrameworkCore;

namespace PANiXiDA.TacticalHeroes.Modules.Players.Infrastructure.Persistence.Core;

public sealed class PlayersReadDbContext(
    DbContextOptions<PlayersReadDbContext> options)
    : ReadDbContext<PlayersReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;
}
