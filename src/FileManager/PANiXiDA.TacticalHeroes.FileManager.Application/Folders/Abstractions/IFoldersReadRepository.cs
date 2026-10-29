using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Folders.GetList;

namespace PANiXiDA.TacticalHeroes.FileManager.Application.Folders.Abstractions;

public interface IFoldersReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<FolderListItemReadModel>> GetPageAsync(
        FoldersFilter filter,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken);
}
