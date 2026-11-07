using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

public sealed class FileManagerWriteDbContext : WriteDbContext<FileManagerWriteDbContext>
{
    public FileManagerWriteDbContext(
        DbContextOptions<FileManagerWriteDbContext> options,
        IEnumerable<IInterceptor> interceptors) : base(options, interceptors)
    {
        ChangeTracker.CascadeDeleteTiming = CascadeTiming.OnSaveChanges;
    }

    protected override bool UseContextNameAsSchema => true;
}
