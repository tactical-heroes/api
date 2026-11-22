using PANiXiDA.TacticalHeroes.Modules.FileManager.Application.Folders.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.FileManager.Presentation.Features.Folders.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetFoldersMapper
{
    [MapPropertyFromSource(nameof(GetFoldersQuery.Filter))]
    internal static partial GetFoldersQuery ToQuery(
        GetFoldersRequest request,
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters);

    internal static partial PaginationResult<FolderListItemResponse> ToResponse(
        PaginationResult<FolderListItemReadModel> page);
}
