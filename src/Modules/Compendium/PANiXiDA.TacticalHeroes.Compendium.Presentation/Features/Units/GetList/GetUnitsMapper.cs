using PANiXiDA.TacticalHeroes.Modules.Compendium.Application.Units.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Modules.Compendium.Presentation.Features.Units.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetUnitsMapper
{
    [MapPropertyFromSource(nameof(GetUnitsQuery.PaginationParameters))]
    internal static partial GetUnitsQuery ToQuery(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters);

    internal static partial PaginationResult<UnitListItemResponse> ToResponse(
        PaginationResult<UnitListItemReadModel> page);
}
