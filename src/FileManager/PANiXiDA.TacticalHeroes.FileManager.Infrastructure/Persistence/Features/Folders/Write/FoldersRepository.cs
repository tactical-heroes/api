using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Write;

public sealed class FoldersRepository(
    FileManagerWriteDbContext dbContext)
    :
        EfRepository<FileManagerWriteDbContext, FolderId, Folder>(
            dbContext),
        IFoldersRepository
{
    private const string HierarchyLockName = $"{nameof(FileManager)}:FolderHierarchy";

    private readonly DatabaseFacade _database = dbContext.Database;

    public async Task LockHierarchyAsync(CancellationToken cancellationToken)
    {
        if (_database.CurrentTransaction is null)
        {
            throw new InvalidOperationException("Locking the folder hierarchy requires an active transaction.");
        }

        await _database.ExecuteSqlInterpolatedAsync(
            $"SELECT pg_advisory_xact_lock(hashtextextended({HierarchyLockName}, 0))", cancellationToken);
    }

    public Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken)
    {
        return Query
            .Where(folder => folder.ParentId == parentId)
            .ToListAsync(cancellationToken);
    }
}
