using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetHeroesMapper
{
    internal static GetHeroesQuery ToQuery(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters)
    {
        return new GetHeroesQuery(paginationParameters, sortingParameters);
    }

    internal static partial PaginationResult<HeroListItemResponse> ToResponse(
        PaginationResult<HeroListItemReadModel> page);
}
