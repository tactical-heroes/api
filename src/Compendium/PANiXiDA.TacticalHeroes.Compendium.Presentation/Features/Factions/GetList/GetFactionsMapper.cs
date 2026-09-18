using PANiXiDA.TacticalHeroes.Compendium.Application.Factions.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Factions.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetFactionsMapper
{
    internal static GetFactionsQuery ToQuery(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters)
    {
        return new GetFactionsQuery(paginationParameters, sortingParameters);
    }

    internal static partial PaginationResult<FactionListItemResponse> ToResponse(
        PaginationResult<FactionListItemReadModel> page);
}
