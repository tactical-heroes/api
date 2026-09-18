using PANiXiDA.TacticalHeroes.Compendium.Application.Units.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Units.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetUnitsMapper
{
    internal static GetUnitsQuery ToQuery(
        PaginationParameters pagination,
        SortingParameters sorting)
    {
        return new GetUnitsQuery(pagination, sorting);
    }

    internal static partial PaginationResult<UnitListItemResponse> ToResponse(
        PaginationResult<UnitListItemReadModel> page);
}
