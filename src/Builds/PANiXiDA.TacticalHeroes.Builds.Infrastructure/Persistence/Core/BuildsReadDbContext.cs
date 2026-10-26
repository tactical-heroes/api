using Microsoft.EntityFrameworkCore;

namespace PANiXiDA.TacticalHeroes.Builds.Infrastructure.Persistence.Core;

public sealed class BuildsReadDbContext(
    DbContextOptions<BuildsReadDbContext> options)
    : ReadDbContext<BuildsReadDbContext>(options)
{
    protected override bool UseContextNameAsSchema => true;
}
