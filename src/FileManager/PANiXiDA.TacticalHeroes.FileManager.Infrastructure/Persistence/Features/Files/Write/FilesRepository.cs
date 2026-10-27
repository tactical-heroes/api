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
    public async IAsyncEnumerable<File> GetPendingPersonalUploadsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var reconciliationOptions = options.Value;
        var createdBefore = timeProvider.GetUtcNow().Subtract(reconciliationOptions.MinimumAge).UtcDateTime;
        var query = Query.Where(file =>
            file.Type == FileType.Personal
                && file.Status == FileStatus.PendingUpload
                && EF.Property<DateTime>(file, EfConstants.CreatedAt) < createdBefore);
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
                .Take(reconciliationOptions.BatchSize)
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
