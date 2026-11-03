using Microsoft.EntityFrameworkCore;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Folders.Write;

public sealed class FoldersRepository(
    FileManagerWriteDbContext dbContext)
    :
        EfRepository<FileManagerWriteDbContext, FolderId, Folder>(
            dbContext),
        IFoldersRepository
{
    private readonly DbSet<File> _files = dbContext.Set<File>();

    public Task<List<Folder>> GetChildrenAsync(FolderId parentId, CancellationToken cancellationToken)
    {
        return Query
            .Where(folder => folder.ParentId == parentId)
            .ToListAsync(cancellationToken);
    }

    public Task<List<Folder>> GetDeletedWithActiveContentsAsync(CancellationToken cancellationToken)
    {
        return Query
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(folder => EF.Property<DateTime?>(folder, EfConstants.DeletedAt) != null
                && (DbSet.Any(child => child.ParentId == folder.Id
                    && EF.Property<DateTime?>(child, EfConstants.DeletedAt) == null)
                    || _files.Any(file => file.FolderId == folder.Id
                        && EF.Property<DateTime?>(file, EfConstants.DeletedAt) == null
                        && file.Status != FileStatus.Deleting)))
            .ToListAsync(cancellationToken);
    }
}
