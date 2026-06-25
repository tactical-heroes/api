using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

using PANiXiDA.TacticalHeroes.Identity.Domain.Roles;
using PANiXiDA.TacticalHeroes.Identity.Domain.Users;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Roles.Write;
using PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Features.Users.Write;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityWriteDbContext(
    DbContextOptions<IdentityWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<IdentityWriteDbContext>(options, interceptors)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Role> Roles => Set<Role>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseOpenIddict<Guid>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new RoleConfiguration());
        modelBuilder.ApplyConfiguration(new UserConfiguration());

        modelBuilder.UseOpenIddict<Guid>();
    }
}
