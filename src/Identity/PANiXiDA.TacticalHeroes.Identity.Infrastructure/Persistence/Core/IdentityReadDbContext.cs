using Microsoft.EntityFrameworkCore;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityReadDbContext(
    DbContextOptions<IdentityReadDbContext> options)
    : ReadDbContext<IdentityReadDbContext>(options)
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseOpenIddict<Guid>();
    }
}
