using PANiXiDA.Core.Application.Authentication.Abstractions;
using PANiXiDA.Core.Application.Storage;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Files.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDownloadUrl;

public sealed class GetPersonalFileDownloadUrlHandler(
    IFilesReadRepository filesReadRepository,
    IFileStorage fileStorage,
    ICurrentUser currentUser)
    : IQueryHandler<GetPersonalFileDownloadUrlQuery, Result<PersonalFileDownloadUrlReadModel>>, IRequireAuthorization
{
    public async Task<Result<PersonalFileDownloadUrlReadModel>> HandleAsync(
        GetPersonalFileDownloadUrlQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || userId == Guid.Empty)
        {
            return Result.Failure<PersonalFileDownloadUrlReadModel>(
                Error.Unauthorized("A user account is required."));
        }

        var file = await filesReadRepository.GetPersonalDownloadByIdAsync(
            id: query.Id,
            userId: userId,
            cancellationToken: cancellationToken);
        if (file is null)
        {
            return Result.Failure<PersonalFileDownloadUrlReadModel>(Error.NotFound("File was not found."));
        }

        if (file.Status != FileStatus.Ready.Name || string.IsNullOrWhiteSpace(file.ContentType))
        {
            return Result.Failure<PersonalFileDownloadUrlReadModel>(Error.Conflict("File is not ready for download."));
        }

        var download = await fileStorage.GetPresignedDownloadUrlAsync(
            key: file.StorageKey,
            fileName: file.Name,
            contentType: file.ContentType,
            cancellationToken: cancellationToken);

        return Result.Success(new PersonalFileDownloadUrlReadModel(download.Url, download.ExpiresAt));
    }
}
