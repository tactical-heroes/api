using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Policies;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;

public sealed class GetFoldersHandler(
    IFoldersReadRepository foldersReadRepository,
    FolderAccessPolicy folderAccessPolicy)
    : IQueryHandler<GetFoldersQuery, Result<PaginationResult<FolderListItemReadModel>>>, IRequireAuthorization
{
    public async Task<Result<PaginationResult<FolderListItemReadModel>>> HandleAsync(
        GetFoldersQuery query,
        CancellationToken cancellationToken)
    {
        var filter = folderAccessPolicy.ApplyReadRestrictions(query.Filter);
        if (filter.IsFailure)
        {
            return Result.Failure<PaginationResult<FolderListItemReadModel>>(filter.Errors);
        }

        var folders = await foldersReadRepository.GetPageAsync(
            filter: filter.Value,
            paginationParameters: query.PaginationParameters,
            sortingParameters: query.SortingParameters,
            cancellationToken: cancellationToken);

        return Result.Success(folders);
    }
}
