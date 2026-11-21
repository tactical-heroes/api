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
    public Task<Folder?> GetUntrackedByIdAsync(FolderId id, CancellationToken cancellationToken)
    {
        return Query.AsNoTracking().SingleOrDefaultAsync(folder => folder.Id == id, cancellationToken);
    }

    public Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken)
    {
        return Query
            .Where(folder => folder.ParentId == parentId)
            .ToListAsync(cancellationToken);
    }
}
