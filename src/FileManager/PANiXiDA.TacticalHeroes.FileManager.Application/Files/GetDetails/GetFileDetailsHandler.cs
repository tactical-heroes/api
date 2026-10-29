using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetDetails;

public sealed class GetFileDetailsHandler(
    IFilesReadRepository filesReadRepository,
    ICurrentUser currentUser,
    FileAccessPolicy fileAccessPolicy)
    : IQueryHandler<GetFileDetailsQuery, Result<FileDetailsReadModel>>, IRequireAuthorization
{
    public async Task<Result<FileDetailsReadModel>> HandleAsync(
        GetFileDetailsQuery query,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated || currentUser.UserId is null)
        {
            return Result.Failure<FileDetailsReadModel>(Error.Unauthorized("A user account is required."));
        }

        var file = await filesReadRepository.GetDetailsByIdAsync(query.Id, cancellationToken);
        return file is null || !fileAccessPolicy.CanRead(new FilesFilter(Type: file.Type, UserId: file.UserId))
            ? Result.Failure<FileDetailsReadModel>(Error.NotFound("File was not found."))
            : Result.Success(file);
    }
}
