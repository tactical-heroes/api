using Microsoft.Extensions.Logging;

using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;

using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

using File = PANiXiDA.TacticalHeroes.FileManager.Domain.Files.File;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.ReconcilePersonalUploads;

public sealed class ReconcilePersonalFileUploadsHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    IUnitOfWork unitOfWork,
    ILogger<ReconcilePersonalFileUploadsHandler> logger)
    : ICommandHandler<ReconcilePersonalFileUploadsCommand, Result>
{
    public async Task<Result> HandleAsync(
        ReconcilePersonalFileUploadsCommand command,
        CancellationToken cancellationToken)
    {
        await foreach (var file in filesRepository.GetPendingPersonalUploadsAsync(cancellationToken))
        {
            await ReconcileFileAsync(file, cancellationToken);
        }

        return Result.Success();
    }

    private async Task ReconcileFileAsync(
        File file,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        FileMetadata? metadata;
        try
        {
            metadata = await fileStorage.GetMetadataAsync(file.StorageKey.Value, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Could not inspect personal file {FileId}; will retry", file.Id.Value);
            return;
        }

        if (metadata is null)
        {
            return;
        }

        var contentType = FileContentType.Create(metadata.ContentType);
        var size = FileSize.Create(metadata.Size);
        if (contentType.IsFailure || size.IsFailure)
        {
            logger.LogWarning("Stored content for personal file {FileId} has invalid metadata", file.Id.Value);
            return;
        }

        var result = file.CompleteUpload(contentType: contentType.Value, size: size.Value);
        if (result.IsSuccess)
        {
            filesRepository.Update(aggregateRoot: file);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            logger.LogInformation("Reconciled personal file {FileId}", file.Id.Value);
        }
    }
}
