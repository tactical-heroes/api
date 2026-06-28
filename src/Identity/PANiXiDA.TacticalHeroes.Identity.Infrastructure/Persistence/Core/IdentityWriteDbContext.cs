using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Identity.Infrastructure.Persistence.Core;

public sealed class IdentityWriteDbContext(
    DbContextOptions<IdentityWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<IdentityWriteDbContext>(options, interceptors)
{
    protected override bool UseContextNameAsSchema => true;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        optionsBuilder.UseNpgsql(options =>
            options.MigrationsHistoryTable("__ef_migrations_history", "identity"));
        optionsBuilder.UseOpenIddict<Guid>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // OpenIddict entities must exist before base schema and naming conventions run.
        modelBuilder.UseOpenIddict<Guid>();

        base.OnModelCreating(modelBuilder);
    }
}
