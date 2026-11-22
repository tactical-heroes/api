using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.Read.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.Read.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetHeroesMapper
{
    [MapPropertyFromSource(nameof(GetHeroesQuery.PaginationParameters))]
    internal static partial GetHeroesQuery ToQuery(
        PaginationParameters paginationParameters,
        SortingParameters sortingParameters);

    internal static partial PaginationResult<HeroListItemResponse> ToResponse(
        PaginationResult<HeroListItemReadModel> page);
}
