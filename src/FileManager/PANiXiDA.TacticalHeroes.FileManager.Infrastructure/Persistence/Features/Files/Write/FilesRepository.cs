using System.Runtime.CompilerServices;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;

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
    private readonly FileManagerWriteDbContext _dbContext = dbContext;

    public async IAsyncEnumerable<File> GetPendingPersonalUploadsAsync(
        [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        var reconciliationOptions = options.Value;
        var createdBefore = timeProvider.GetUtcNow().Subtract(reconciliationOptions.MinimumAge).UtcDateTime;
        var afterId = Guid.Empty;
        while (true)
        {
            var files = await _dbContext.Set<File>()
                .FromSqlInterpolated(
                    $"""
                    SELECT *, xmin FROM file_manager.files
                    WHERE type = {FileType.Personal.Name} AND status = {FileStatus.PendingUpload.Name}
                        AND created_at < {createdBefore} AND id > {afterId}
                    ORDER BY id
                    LIMIT {reconciliationOptions.BatchSize}
                    """)
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

            afterId = files[^1].Id.Value;
        }
    }
}
