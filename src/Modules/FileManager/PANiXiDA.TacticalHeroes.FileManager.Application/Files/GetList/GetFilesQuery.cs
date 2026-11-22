using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.Common.Filters;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetList;

public sealed record GetFilesQuery(
    FilesFilter Filter,
    PaginationParameters PaginationParameters,
    SortingParameters SortingParameters)
    : IQuery<Result<PaginationResult<FileListItemReadModel>>>;
