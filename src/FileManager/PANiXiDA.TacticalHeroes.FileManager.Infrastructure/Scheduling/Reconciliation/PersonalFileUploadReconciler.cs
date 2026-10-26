using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Core;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Persistence.Features.Files.Read.DbModels;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Storage;

namespace PANiXiDA.TacticalHeroes.FileManager.Infrastructure.Scheduling.Reconciliation;

internal sealed class PersonalFileUploadReconciler(
    FileManagerReadDbContext dbContext,
    S3FileMetadataReader metadataReader,
    IServiceScopeFactory scopeFactory,
    TimeProvider timeProvider,
    IOptions<FileUploadReconciliationOptions> options,
    ILogger<PersonalFileUploadReconciler> logger)
    : IPersonalFileUploadReconciler
{
    public async Task ReconcileAsync(CancellationToken cancellationToken)
    {
        var reconciliationOptions = options.Value;
        var createdBefore = timeProvider.GetUtcNow().Subtract(reconciliationOptions.MinimumAge).UtcDateTime;
        var afterId = Guid.Empty;
        while (true)
        {
            var files = await dbContext.Set<FileReadDbModel>()
                .AsNoTracking()
                .Where(file =>
                    file.Type == FileType.Personal.Name
                        && file.Status == FileStatus.PendingUpload.Name
                        && file.CreatedAt < createdBefore
                        && file.Id.CompareTo(afterId) > 0)
                .OrderBy(file => file.Id)
                .Select(file => new { file.Id, file.StorageKey })
                .Take(reconciliationOptions.BatchSize)
                .ToListAsync(cancellationToken);
            if (files.Count == 0)
            {
                return;
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(reconciliationOptions.StorageRequestTimeout);
                    var metadata = await metadataReader.GetMetadataAsync(file.StorageKey, timeout.Token);
                    if (metadata is null)
                    {
                        logger.LogInformation("Personal file {FileId} is still awaiting uploaded content", file.Id);
                        continue;
                    }

                    var contentType = FileContentType.Create(metadata.ContentType);
                    var size = FileSize.Create(metadata.Size);
                    if (contentType.IsFailure || size.IsFailure)
                    {
                        logger.LogWarning("Stored content for personal file {FileId} has invalid metadata", file.Id);
                        continue;
                    }

                    await using var scope = scopeFactory.CreateAsyncScope();
                    var result = await scope.ServiceProvider.GetRequiredService<IFileUploadFinalizer>().CompleteAsync(
                        id: FileId.Create(file.Id).Value,
                        contentType: contentType.Value,
                        size: size.Value,
                        cancellationToken: cancellationToken);
                    if (result.IsSuccess)
                    {
                        logger.LogInformation("Reconciled personal file {FileId}", file.Id);
                    }
                    else
                    {
                        logger.LogWarning("Personal file {FileId} cannot be finalized in its current state", file.Id);
                    }
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Could not reconcile personal file {FileId}; will retry", file.Id);
                }
            }

            afterId = files[^1].Id;
        }
    }
}
