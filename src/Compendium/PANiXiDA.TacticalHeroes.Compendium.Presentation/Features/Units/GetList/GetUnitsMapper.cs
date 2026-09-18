using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetUnitsMapper
{
    internal static GetUnitsQuery ToQuery(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters)
    {
        return new GetUnitsQuery(paginationParameters, sortingParameters);
    }

    internal static partial PaginationResult<UnitListItemResponse> ToResponse(
        PaginationResult<UnitListItemReadModel> page);
}
