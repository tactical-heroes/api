using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.GetList;

public sealed record GetFoldersQuery(
    FoldersFilter Filter,
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<FolderListItemReadModel>>>;
