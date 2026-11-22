using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Modules.Builds.Infrastructure.Persistence.Core;

public sealed class BuildsWriteDbContext(
    DbContextOptions<BuildsWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<BuildsWriteDbContext>(options, interceptors)
{
    protected override bool UseContextNameAsSchema => true;
}
