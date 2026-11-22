using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Infrastructure.Persistence.Features.Folders.Write;

public sealed class PostgreSqlFolderHierarchyLock(FileManagerWriteDbContext dbContext) : IFolderHierarchyLock
{
    private const string HierarchyLockName = $"{nameof(FileManager)}:FolderHierarchy";

    public async Task AcquireAsync(CancellationToken cancellationToken)
    {
        if (dbContext.Database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Locking the folder hierarchy requires an active transaction.");
        }

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({HierarchyLockName}, 0))", cancellationToken);
    }
}
