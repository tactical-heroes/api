using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Common.Filters;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.GetDetails;
using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.GetList;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Abstractions;

public interface IFoldersReadRepository : IReadRepository<Guid>
{
    Task<PaginationResult<FolderListItemReadModel>> GetPageAsync(
        FoldersFilter filter,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters,
        CancellationToken cancellationToken);

    Task<FolderDetailsReadModel?> GetDetailsByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasAncestorAsync(Guid id, Guid ancestorId, CancellationToken cancellationToken);
    Task<bool> HasContentsAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Guid>> GetDeletedIdsWithActiveContentsAsync(CancellationToken cancellationToken);
}
