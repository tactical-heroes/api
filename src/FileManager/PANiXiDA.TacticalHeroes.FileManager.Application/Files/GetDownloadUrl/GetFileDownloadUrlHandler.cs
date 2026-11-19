using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;
using PANiXiDA.Core.Application.Storage.Models;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.ValueObjects;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDownloadUrl;

public sealed class GetFileDownloadUrlHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    ICurrentUser currentUser,
    FileAccessPolicy fileAccessPolicy)
    : IQueryHandler<GetFileDownloadUrlQuery, Result<PresignedDownloadUrl>>, IRequireAuthorization
{
    public async Task<Result<PresignedDownloadUrl>> HandleAsync(
        GetFileDownloadUrlQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<PresignedDownloadUrl>(
                Error.Unauthorized("A user account is required."));
        }

        var fileId = FileId.Create(query.Id);
        if (fileId.IsFailure)
        {
            return Result.Failure<PresignedDownloadUrl>(fileId.Errors);
        }

        var file = await filesRepository.GetByIdAsync(id: fileId.Value, cancellationToken: cancellationToken);
        if (file is null || !fileAccessPolicy.CanRead(file))
        {
            return Result.Failure<PresignedDownloadUrl>(Error.NotFound("File was not found."));
        }

        var contentTypeResult = file.GetDownloadContentType();
        if (contentTypeResult.IsFailure)
        {
            return Result.Failure<PresignedDownloadUrl>(contentTypeResult.Errors);
        }

        var versionId = file.VersionId;
        if (versionId is null)
        {
            var metadata = await fileStorage.GetMetadataAsync(file.StorageKey.Value, cancellationToken);
            var storedVersion = FileVersionId.Create(metadata?.VersionId);
            if (file.Status != FileStatus.Ready
                || storedVersion.IsFailure
                || metadata!.Metadata.ContainsKey(FileMetadataNames.ContentUpdateId))
            {
                return Result.Failure<PresignedDownloadUrl>(
                    Error.Conflict("File content has no active storage version."));
            }

            versionId = storedVersion.Value;
        }

        var download = await fileStorage.GetPresignedDownloadUrlAsync(
            key: file.StorageKey.Value,
            versionId: versionId.Value,
            fileName: file.Name.Value,
            contentType: contentTypeResult.Value.Value,
            cancellationToken: cancellationToken);

        return Result.Success(download);
    }
}
