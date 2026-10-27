using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDownloadUrl;

public sealed class GetFileDownloadUrlHandler(
    IFilesRepository filesRepository,
    IFileStorage fileStorage,
    ICurrentUser currentUser,
    FileAccessPolicy fileAccessPolicy)
    : IQueryHandler<GetFileDownloadUrlQuery, Result<FileDownloadUrlReadModel>>, IRequireAuthorization
{
    public async Task<Result<FileDownloadUrlReadModel>> HandleAsync(
        GetFileDownloadUrlQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || userId == Guid.Empty)
        {
            return Result.Failure<FileDownloadUrlReadModel>(
                Error.Unauthorized("A user account is required."));
        }

        var fileId = FileId.Create(query.Id);
        if (fileId.IsFailure)
        {
            return Result.Failure<FileDownloadUrlReadModel>(fileId.Errors);
        }

        var file = await filesRepository.GetByIdAsync(id: fileId.Value, cancellationToken: cancellationToken);
        if (file is null || !fileAccessPolicy.CanRead(file))
        {
            return Result.Failure<FileDownloadUrlReadModel>(Error.NotFound("File was not found."));
        }

        if (file.Status != FileStatus.Ready || file.ContentType is not { } contentType)
        {
            return Result.Failure<FileDownloadUrlReadModel>(Error.Conflict("File is not ready for download."));
        }

        var download = await fileStorage.GetPresignedDownloadUrlAsync(
            key: file.StorageKey.Value,
            fileName: file.Name.Value,
            contentType: contentType.Value,
            cancellationToken: cancellationToken);

        return Result.Success(new FileDownloadUrlReadModel(download.Url, download.ExpiresAt));
    }
}
