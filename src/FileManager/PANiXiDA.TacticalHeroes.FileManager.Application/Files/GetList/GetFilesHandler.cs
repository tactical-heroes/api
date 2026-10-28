using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;
using PANiXiDA.TacticalHeroes.FileManager.Domain.Common.Enumerations;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

public sealed class GetFilesHandler(
    IFilesReadRepository filesReadRepository,
    ICurrentUser currentUser,
    FileAccessPolicy fileAccessPolicy)
    : IQueryHandler<GetFilesQuery, Result<PaginationResult<FileListItemReadModel>>>, IRequireAuthorization
{
    public async Task<Result<PaginationResult<FileListItemReadModel>>> HandleAsync(
        GetFilesQuery query,
        CancellationToken cancellationToken)
    {
        if (currentUser.UserId is null)
        {
            return Result.Failure<PaginationResult<FileListItemReadModel>>(
                Error.Unauthorized("A user account is required."));
        }

        var type = FileType.Create(query.Filter.Type);
        if (type.IsFailure)
        {
            return Result.Failure<PaginationResult<FileListItemReadModel>>(type.Errors);
        }

        var access = fileAccessPolicy.GetReadAccess(type.Value);
        if (access.IsFailure)
        {
            return Result.Failure<PaginationResult<FileListItemReadModel>>(access.Errors);
        }

        var files = await filesReadRepository.GetPageAsync(
            filter: query.Filter with { Type = type.Value.Name },
            access: access.Value,
            paginationParameters: query.PaginationParameters,
            sortingParameters: query.SortingParameters,
            cancellationToken: cancellationToken);

        return Result.Success(files);
    }
}
