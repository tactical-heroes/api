using System.Runtime.CompilerServices;

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
    private const int DeletionBatchSize = 100;

    private readonly DbSet<File> _files = dbContext.Set<File>();

    public Task<List<Folder>> GetChildrenBatchAsync(FolderId parentId, CancellationToken cancellationToken)
    {
        return Query
            .Where(folder => folder.ParentId == parentId)
            .OrderBy(folder => folder.Id)
            .Take(DeletionBatchSize)
            .ToListAsync(cancellationToken);
    }

    public async IAsyncEnumerable<Folder> GetDeletedWithActiveContentsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var query = Query
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(folder => EF.Property<DateTime?>(folder, EfConstants.DeletedAt) != null
                && (DbSet.Any(child => child.ParentId == folder.Id
                    && EF.Property<DateTime?>(child, EfConstants.DeletedAt) == null)
                    || _files.Any(file => file.FolderId == folder.Id
                        && EF.Property<DateTime?>(file, EfConstants.DeletedAt) == null
                        && file.Status != FileStatus.Deleting)));
        FolderId? afterId = null;
        while (true)
        {
            var pageQuery = query;
            if (afterId is { } cursor)
            {
                pageQuery = pageQuery.Where(folder => EF.Functions.GreaterThan(
                    ValueTuple.Create(folder.Id),
                    ValueTuple.Create(cursor)));
            }

            var folders = await pageQuery
                .OrderBy(folder => folder.Id)
                .Take(DeletionBatchSize)
                .ToListAsync(cancellationToken);
            if (folders.Count == 0)
            {
                yield break;
            }

            foreach (var folder in folders)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return folder;
            }

            afterId = folders[^1].Id;
        }
    }
}
