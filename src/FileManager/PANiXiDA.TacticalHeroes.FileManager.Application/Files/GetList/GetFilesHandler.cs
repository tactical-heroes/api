using PANiXiDA.Core.Application.Authentication.Abstractions;

using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Abstractions;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Policies;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

public sealed class GetFilesHandler(
    IFilesReadRepository filesReadRepository,
    FileAccessPolicy fileAccessPolicy)
    : IQueryHandler<GetFilesQuery, Result<PaginationResult<FileListItemReadModel>>>, IRequireAuthorization
{
    public async Task<Result<PaginationResult<FileListItemReadModel>>> HandleAsync(
        GetFilesQuery query,
        CancellationToken cancellationToken)
    {
        var filter = fileAccessPolicy.ApplyReadRestrictions(query.Filter);
        if (filter.IsFailure)
        {
            return Result.Failure<PaginationResult<FileListItemReadModel>>(filter.Errors);
        }

        var files = await filesReadRepository.GetPageAsync(
            filter: filter.Value,
            paginationParameters: query.PaginationParameters,
            sortingParameters: query.SortingParameters,
            cancellationToken: cancellationToken);

        return Result.Success(files);
    }
}
