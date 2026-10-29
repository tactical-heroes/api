using PANiXiDA.TacticalHeroes.FileManager.Application.Files.Common.Filters;
using PANiXiDA.TacticalHeroes.FileManager.Application.Files.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.FileManager.Presentation.Features.Files.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetFilesMapper
{
    [MapPropertyFromSource(nameof(GetFilesQuery.Filter))]
    internal static partial GetFilesQuery ToQuery(
        GetFilesRequest request,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters);

    internal static partial PaginationResult<FileListItemResponse> ToResponse(
        PaginationResult<FileListItemReadModel> page);

    [MapperIgnoreTarget(nameof(FilesFilter.Types))]
    private static partial FilesFilter ToFilter(GetFilesRequest request);
}
