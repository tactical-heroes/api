using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetPersonalDetails;

public sealed class GetPersonalFileDetailsHandler(
    IFilesReadRepository filesReadRepository,
    ICurrentUser currentUser)
    : IQueryHandler<GetPersonalFileDetailsQuery, Result<PersonalFileDetailsReadModel>>, IRequireAuthorization
{
    public async Task<Result<PersonalFileDetailsReadModel>> HandleAsync(
        GetPersonalFileDetailsQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is not { } userId || userId == Guid.Empty)
        {
            return Result.Failure<PersonalFileDetailsReadModel>(
                error: Error.Unauthorized(message: "A user account is required."));
        }

        var file = await filesReadRepository.GetPersonalDetailsByIdAsync(
            id: query.Id,
            userId: userId,
            cancellationToken: cancellationToken);

        return file is null
            ? Result.Failure<PersonalFileDetailsReadModel>(
                error: Error.NotFound(message: "File was not found."))
            : Result.Success(value: file);
    }
}
