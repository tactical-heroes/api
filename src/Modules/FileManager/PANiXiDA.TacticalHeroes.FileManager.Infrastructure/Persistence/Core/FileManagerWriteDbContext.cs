using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Core;

public sealed class FileManagerWriteDbContext(
    DbContextOptions<FileManagerWriteDbContext> options,
    IEnumerable<IInterceptor> interceptors)
    : WriteDbContext<FileManagerWriteDbContext>(options, interceptors)
{
    protected override bool UseContextNameAsSchema => true;
}
