using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityRoles;
using PANiXiDA.TacticalHeroes.Identity.Domain.IdentityUsers;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityWriteDbContext(
    DbContextOptions<IdentityWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<IdentityWriteDbContext>(options, interceptors)
{
    public DbSet<IdentityUser> IdentityUsers => Set<IdentityUser>();

    public DbSet<IdentityRole> IdentityRoles => Set<IdentityRole>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseOpenIddict<Guid>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseOpenIddict<Guid>();
    }
}
