using Microsoft.EntityFrameworkCore;

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
    private const int DeletionBatchSize = 100;

    public Task<List<Folder>> GetChildrenBatchAsync(FolderId parentId, CancellationToken cancellationToken)
    {
        return Query
            .Where(folder => folder.ParentId == parentId)
            .OrderBy(folder => folder.Id)
            .Take(DeletionBatchSize)
            .ToListAsync(cancellationToken);
    }
}
