using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Write;

public sealed class FilesRepository(
    FileManagerWriteDbContext dbContext,
    IOptions<FileUploadReconciliationOptions> options,
    TimeProvider timeProvider)
    :
        EfRepository<FileManagerWriteDbContext, FileId, File>(
            dbContext),
        IFilesRepository
{
    public IAsyncEnumerable<File> GetPendingUploadsAsync(CancellationToken cancellationToken)
    {
        var reconciliationOptions = options.Value;
        var now = timeProvider.GetUtcNow();
        var createdBefore = now.Subtract(reconciliationOptions.MinimumAge).UtcDateTime;
        var expiresBefore = now.Subtract(reconciliationOptions.ExpirationAge).UtcDateTime;

        return GetFilesAsync(
            Query.Where(file => file.Status == FileStatus.PendingUpload
                && EF.Property<DateTime>(file, EfConstants.CreatedAt) >= expiresBefore
                && EF.Property<DateTime>(file, EfConstants.CreatedAt) < createdBefore),
            cancellationToken);
    }

    public IAsyncEnumerable<File> GetExpiredUploadsAsync(CancellationToken cancellationToken)
    {
        var expiresBefore = timeProvider.GetUtcNow().Subtract(options.Value.ExpirationAge).UtcDateTime;

        return GetFilesAsync(
            Query.Where(file => file.Status == FileStatus.PendingUpload
                && EF.Property<DateTime>(file, EfConstants.CreatedAt) < expiresBefore),
            cancellationToken);
    }

    public IAsyncEnumerable<File> GetFilesForDeletionAsync(CancellationToken cancellationToken)
    {
        return GetFilesAsync(
            Query.Where(file => file.Status == FileStatus.Deleting),
            cancellationToken);
    }

    public IAsyncEnumerable<File> GetDeletedFilesAsync(CancellationToken cancellationToken)
    {
        return GetFilesAsync(
            Query.IgnoreQueryFilters().Where(file => EF.Property<DateTime?>(file, EfConstants.DeletedAt) != null),
            cancellationToken);
    }

    private async IAsyncEnumerable<File> GetFilesAsync(
        IQueryable<File> query,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var batchSize = options.Value.BatchSize;
        FileId? afterId = null;
        while (true)
        {
            var pageQuery = query;
            if (afterId is { } cursor)
            {
                pageQuery = pageQuery.Where(file => EF.Functions.GreaterThan(
                    ValueTuple.Create(file.Id),
                    ValueTuple.Create(cursor)));
            }

            var files = await pageQuery
                .OrderBy(file => file.Id)
                .Take(batchSize)
                .ToListAsync(cancellationToken);
            if (files.Count == 0)
            {
                yield break;
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                yield return file;
            }

            afterId = files[^1].Id;
        }
    }
}
