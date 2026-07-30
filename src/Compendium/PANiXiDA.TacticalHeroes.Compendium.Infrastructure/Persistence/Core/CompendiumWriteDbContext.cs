using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Compendium.Infrastructure.Persistence.Core;

public sealed class CompendiumWriteDbContext(
    DbContextOptions<CompendiumWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<CompendiumWriteDbContext>(options: options, interceptors: interceptors)
{
    protected override bool UseContextNameAsSchema => true;
}
