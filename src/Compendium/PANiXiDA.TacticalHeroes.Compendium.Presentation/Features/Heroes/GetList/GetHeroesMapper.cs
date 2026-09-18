using PANiXiDA.TacticalHeroes.Compendium.Application.Heroes.GetList;

using Riok.Mapperly.Abstractions;

namespace PANiXiDA.TacticalHeroes.Compendium.Presentation.Features.Heroes.GetList;

[Mapper(IncludedConstructors = MemberVisibility.All)]
internal static partial class GetHeroesMapper
{
    internal static GetHeroesQuery ToQuery(
        PaginationParameters pagination,
        SortingParameters sorting)
    {
        return new GetHeroesQuery(pagination, sorting);
    }

    internal static partial PaginationResult<HeroListItemResponse> ToResponse(
        PaginationResult<HeroListItemReadModel> page);
}
