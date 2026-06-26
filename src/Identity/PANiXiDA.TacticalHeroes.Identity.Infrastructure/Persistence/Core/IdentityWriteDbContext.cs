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

        optionsBuilder.UseOpenIddict<Guid>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseOpenIddict<Guid>();
    }
}
