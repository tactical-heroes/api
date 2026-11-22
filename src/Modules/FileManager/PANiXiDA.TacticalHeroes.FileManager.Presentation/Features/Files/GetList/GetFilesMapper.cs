using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Files.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Files.GetList;

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
}
