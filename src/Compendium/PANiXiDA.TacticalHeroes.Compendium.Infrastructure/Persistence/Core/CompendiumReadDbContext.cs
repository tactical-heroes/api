using Microsoft.EntityFrameworkCore;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

public sealed class CompendiumReadDbContext(
    DbContextOptions<CompendiumReadDbContext> options)
    : ReadDbContext<CompendiumReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;
}
