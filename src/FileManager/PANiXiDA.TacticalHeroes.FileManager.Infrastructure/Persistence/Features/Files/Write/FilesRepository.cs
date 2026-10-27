using System.Linq.Expressions;
using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
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
    public IAsyncEnumerable<File> GetPendingPersonalUploadsAsync(CancellationToken cancellationToken)
    {
        var reconciliationOptions = options.Value;
        var now = timeProvider.GetUtcNow();
        var createdBefore = now.Subtract(reconciliationOptions.MinimumAge).UtcDateTime;
        var expiresBefore = now.Subtract(reconciliationOptions.ExpirationAge).UtcDateTime;

        return GetPersonalFilesAsync(
            file => file.Status == FileStatus.PendingUpload
                && EF.Property<DateTime>(file, EfConstants.CreatedAt) >= expiresBefore
                && EF.Property<DateTime>(file, EfConstants.CreatedAt) < createdBefore,
            cancellationToken);
    }

    public IAsyncEnumerable<File> GetExpiredPersonalUploadsAsync(CancellationToken cancellationToken)
    {
        var expiresBefore = timeProvider.GetUtcNow().Subtract(options.Value.ExpirationAge).UtcDateTime;

        return GetPersonalFilesAsync(
            file => file.Status == FileStatus.PendingUpload
                && EF.Property<DateTime>(file, EfConstants.CreatedAt) < expiresBefore,
            cancellationToken);
    }

    public IAsyncEnumerable<File> GetPersonalFilesForDeletionAsync(CancellationToken cancellationToken)
    {
        return GetPersonalFilesAsync(
            file => file.Status == FileStatus.Deleting || file.Status == FileStatus.Deleted,
            cancellationToken);
    }

    private async IAsyncEnumerable<File> GetPersonalFilesAsync(
        Expression<Func<File, bool>> predicate,
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var batchSize = options.Value.BatchSize;
        var query = Query.Where(file => file.Type == FileType.Personal).Where(predicate);
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
