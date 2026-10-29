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
        if (!currentUser.IsAuthenticated || currentUser.UserId is not { } userId)
        {
            return Result.Failure<PaginationResult<FileListItemReadModel>>(
                Error.Unauthorized("A user account is required."));
        }

        var type = FileType.CreateOptional(query.Filter.Type);
        if (type.IsFailure)
        {
            return Result.Failure<PaginationResult<FileListItemReadModel>>(type.Errors);
        }

        var filter = query.Filter with { Type = type.Value?.Name };
        var includesPersonal = type.Value is null || type.Value == FileType.Personal;
        if (includesPersonal
            && filter.UserId is null
            && !fileAccessPolicy.CanRead(filter with { Type = FileType.Personal.Name })
            && fileAccessPolicy.CanRead(filter with { Type = FileType.Personal.Name, UserId = userId }))
        {
            filter = filter with { UserId = userId };
        }

        if (!fileAccessPolicy.CanRead(filter))
        {
            return Result.Failure<PaginationResult<FileListItemReadModel>>(
                Error.Forbidden("Access to files is denied."));
        }

        if (filter.Type is null)
        {
            var canReadPersonal = fileAccessPolicy.CanRead(filter with { Type = FileType.Personal.Name });
            var canReadAvatars = fileAccessPolicy.CanRead(filter with { Type = FileType.Avatar.Name });
            if (!canReadPersonal)
            {
                filter = filter with { Type = FileType.Avatar.Name };
            }
            else if (!canReadAvatars)
            {
                filter = filter with { Type = FileType.Personal.Name };
            }
        }

        var files = await filesReadRepository.GetPageAsync(
            filter: filter,
            paginationParameters: query.PaginationParameters,
            sortingParameters: query.SortingParameters,
            cancellationToken: cancellationToken);

        return Result.Success(files);
    }
}
