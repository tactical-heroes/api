using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Options.FileUploadReconciliation;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

public sealed class ReconcilePersonalFileUploadsHandler(
    IFilesRepository filesRepository,
    IFilesReadRepository filesReadRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    TimeProvider timeProvider,
    IOptions<FileUploadReconciliationOptions> options,
    ILogger<ReconcilePersonalFileUploadsHandler> logger)
    : ICommandHandler<ReconcilePersonalFileUploadsCommand, Result>
{
    public async Task<Result> HandleAsync(
        ReconcilePersonalFileUploadsCommand command,
        CancellationToken cancellationToken)
    {
        var reconciliationOptions = options.Value;
        var createdBefore = timeProvider.GetUtcNow().Subtract(reconciliationOptions.MinimumAge).UtcDateTime;
        var afterId = Guid.Empty;
        while (true)
        {
            var files = await filesReadRepository.GetPendingPersonalUploadsAsync(
                createdBefore: createdBefore,
                afterId: afterId,
                batchSize: reconciliationOptions.BatchSize,
                cancellationToken: cancellationToken);
            if (files.Count == 0)
            {
                return Result.Success();
            }

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                FileMetadata? metadata;
                try
                {
                    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeout.CancelAfter(reconciliationOptions.StorageRequestTimeout);
                    metadata = await fileStorage.GetMetadataAsync(file.StorageKey, timeout.Token);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    throw;
                }
                catch (Exception exception)
                {
                    logger.LogError(exception, "Could not inspect personal file {FileId}; will retry", file.Id);
                    continue;
                }

                if (metadata is null)
                {
                    continue;
                }

                var contentType = FileContentType.Create(metadata.ContentType);
                var size = FileSize.Create(metadata.Size);
                if (contentType.IsFailure || size.IsFailure)
                {
                    logger.LogWarning("Stored content for personal file {FileId} has invalid metadata", file.Id);
                    continue;
                }

                var aggregate = await filesRepository.GetByIdAsync(
                    id: FileId.Create(file.Id).Value,
                    cancellationToken: cancellationToken);
                if (aggregate is null
                    || aggregate.Type != FileType.Personal
                    || aggregate.Status != FileStatus.PendingUpload
                    || aggregate.StorageKey.Value != file.StorageKey)
                {
                    continue;
                }

                var result = aggregate.CompleteUpload(contentType: contentType.Value, size: size.Value);
                if (result.IsSuccess)
                {
                    filesRepository.Update(aggregateRoot: aggregate);
                    await unitOfWork.SaveChangesAsync(cancellationToken);
                    logger.LogInformation("Reconciled personal file {FileId}", file.Id);
                }
            }

            afterId = files[^1].Id;
        }
    }
}
